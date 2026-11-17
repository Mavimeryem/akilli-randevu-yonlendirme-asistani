using System.Text.Json;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;
using Microsoft.Extensions.Options;

namespace AkilliRandevu.Business.YapayZeka;

/// <summary>
/// Claude (Anthropic resmi C# SDK'sı) istemcisi.
///   - Yapılandırılmış çıktı: yanıt verilen JSON şemasına uymak zorunda (output_config.format = json_schema).
///   - Zaman aşımı + SDK'nın kendi yeniden denemesi (429 / 5xx / ağ hatası, en fazla 2 kez).
///   - Güvenlik reddi (stop_reason = "refusal") ayrıca bildirilir; sunucu taraflı yedek model açıksa aynı
///     çağrıda yedek model yanıt verir.
///   - API anahtarı yalnızca ortam değişkeninden gelir ve hiçbir mesaja/loga yazılmaz.
/// </summary>
public sealed class ClaudeYapayZekaIstemcisi : IYapayZekaIstemcisi
{
    private readonly YapayZekaAyarlari _ayar;
    private readonly Lazy<AnthropicClient> _client;

    public ClaudeYapayZekaIstemcisi(IOptions<YapayZekaAyarlari> ayar)
    {
        _ayar = ayar.Value;
        _client = new Lazy<AnthropicClient>(() => new AnthropicClient
        {
            ApiKey = _ayar.EtkinAnahtar,
            Timeout = TimeSpan.FromSeconds(_ayar.ZamanAsimiSaniye),
            MaxRetries = 2
        });
    }

    public bool Yapilandirildi => _ayar.Yapilandirildi;

    public string Model => _ayar.Model;

    public async Task<YzHamYanit> TamamlaAsync(
        string sistem, string kullanici, IReadOnlyDictionary<string, JsonElement> sema, CancellationToken ct)
    {
        if (!Yapilandirildi)
            throw new YzIstemciHatasi("Yapay zekâ servisi yapılandırılmamış.");

        var istek = new MessageCreateParams
        {
            Model = _ayar.Model,
            MaxTokens = _ayar.MaxTokens,
            System = sistem,
            Messages = [new BetaUserMessageParam { Content = kullanici }],
            OutputConfig = new BetaOutputConfig
            {
                Effort = _ayar.Efor switch
                {
                    "medium" => Effort.Medium,
                    "high" => Effort.High,
                    _ => Effort.Low
                },
                Format = new BetaJsonOutputFormat { Schema = sema.ToDictionary(x => x.Key, x => x.Value) }
            }
        };
        if (_ayar.YedekModelEtkin)
        {
            istek = istek with
            {
                Betas = [AnthropicBeta.ServerSideFallback2026_07_01],
                Fallbacks = new Default()
            };
        }

        try
        {
            BetaMessage yanit = await _client.Value.Beta.Messages.Create(istek, cancellationToken: ct);

            if (yanit.StopReason == "refusal")
                return new YzHamYanit(null, Reddedildi: true, Model: yanit.Model, DurmaNedeni: "refusal");

            var metin = string.Concat(yanit.Content.Select(b => b.Value).OfType<BetaTextBlock>().Select(t => t.Text));
            return new YzHamYanit(metin, Reddedildi: false, Model: yanit.Model, DurmaNedeni: yanit.StopReason);
        }
        // En özelden en genele: tekrar denenebilir ve denenemez hatalar ayrı mesajlarla kaydedilir.
        catch (AnthropicUnauthorizedException ex)
        {
            throw new YzIstemciHatasi("Yapay zekâ API anahtarı geçersiz (401).", ex);
        }
        catch (AnthropicRateLimitException ex)
        {
            throw new YzIstemciHatasi("Yapay zekâ hız sınırına takıldı (429); daha sonra tekrar deneyin.", ex);
        }
        catch (Anthropic5xxException ex)
        {
            throw new YzIstemciHatasi("Yapay zekâ servisi geçici olarak yanıt vermiyor (5xx).", ex);
        }
        catch (Anthropic4xxException ex)
        {
            throw new YzIstemciHatasi($"Yapay zekâ isteği reddedildi (4xx): {Kisalt(ex.Message)}", ex);
        }
        catch (AnthropicIOException ex)
        {
            throw new YzIstemciHatasi("Yapay zekâ servisine bağlanılamadı.", ex);
        }
        catch (AnthropicApiException ex)
        {
            throw new YzIstemciHatasi($"Yapay zekâ hatası: {Kisalt(ex.Message)}", ex);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new YzIstemciHatasi($"Yapay zekâ {_ayar.ZamanAsimiSaniye} saniye içinde yanıt vermedi (zaman aşımı).", ex);
        }
    }

    private static string Kisalt(string s) => s.Length > 200 ? s[..200] : s;
}
