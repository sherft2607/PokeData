using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PokeData
{
    // PokeAPI is fully public and unauthenticated — this client carries no token and applies
    // no auth headers, unlike a generated client for a write-capable API.
    public sealed class PokeDataClient
    {
        #region Constructor and setup

        // ONE HttpClient for the whole plugin — shared across every component and every solve.
        private static readonly HttpClient _http;

        // v4.0.0 round 5: a per-endpoint-plus-ID response cache, shared across every component instance
        // (static, same lifetime as _http). A "core lookup + list breakout" pair (e.g. Get Region /
        // Get Region Locations / Get Region Pokedexes) all call the same GET for the same ID — this
        // cache means only the first of them on a given solve actually hits the network; the rest
        // reuse its parsed-JSON response. Only successful responses are cached; a failure is never
        // cached so a transient error doesn't stick.
        private static readonly ConcurrentDictionary<string, Tuple<bool, string, string>> _responseCache
            = new ConcurrentDictionary<string, Tuple<bool, string, string>>(StringComparer.OrdinalIgnoreCase);

        static PokeDataClient()
        {
#if NET48
            // net48 (Rhino 7): the classic HttpClientHandler stack reads TLS version from
            // ServicePointManager — SocketsHttpHandler doesn't exist on .NET Framework.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            _http = new HttpClient
            {
                BaseAddress = new Uri("https://pokeapi.co/"),
                Timeout = TimeSpan.FromSeconds(30)
            };
#else
            // net7.0 / net7.0-windows (Rhino 8): the modern SocketsHttpHandler stack ignores
            // ServicePointManager entirely — TLS protocol selection goes through SslClientAuthenticationOptions.
            var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions
                {
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                }
            };
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://pokeapi.co/"),
                Timeout = TimeSpan.FromSeconds(30)
            };
#endif
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("PokeData-Grasshopper/1.0 (Grasshopper-Plugin)");
        }

        // relativeUrl never starts with '/' — see NewRequest
        private HttpRequestMessage NewRequest(HttpMethod method, string relativeUrl)
        {
            relativeUrl = (relativeUrl ?? "").TrimStart('/');
            return new HttpRequestMessage(method, relativeUrl);
        }

        // Every path segment goes through this — never string-concatenate raw input
        private static string Esc(string value) => Uri.EscapeDataString(value ?? "");

        #endregion

        #region Pokemon methods

        public async Task<Tuple<bool, string, string>> GetPokemonAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Pokemon Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/pokemon/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Type methods

        public async Task<Tuple<bool, string, string>> GetTypeListAsync()
        {
            try
            {
                var req = NewRequest(HttpMethod.Get, "api/v2/type/?limit=100");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetTypeAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Type Name is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/type/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Evolution methods

        public async Task<Tuple<bool, string, string>> GetPokemonSpeciesAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Species Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/pokemon-species/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // evolution_chain.url from a species response is a full absolute URL — fetched as-is
        public async Task<Tuple<bool, string, string>> GetByAbsoluteUrlAsync(string absoluteUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(absoluteUrl)) return Fail("URL is empty.");
                var req = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Move / Ability methods

        public async Task<Tuple<bool, string, string>> GetMoveAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Move Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/move/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetAbilityAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Ability Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/ability/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region v2.1.0: Generation / Item / Nature methods

        public async Task<Tuple<bool, string, string>> GetGenerationAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Generation is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/generation/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetItemAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Item Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/item/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetNatureAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Nature Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/nature/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region v4.0.0: Berry / Location / Machine methods

        public async Task<Tuple<bool, string, string>> GetBerryAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Berry Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/berry/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetLocationAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Location Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/location/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetMachineAsync(int id)
        {
            try
            {
                if (id <= 0) return Fail("Machine ID must be a positive integer.");
                var req = NewRequest(HttpMethod.Get, "api/v2/machine/" + id + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region v4.0.0 round 5: Region / Pokedex / Egg Group / Growth Rate / Stat methods

        public async Task<Tuple<bool, string, string>> GetRegionAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Region Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/region/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetPokedexAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Pokedex Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/pokedex/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetEggGroupAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Egg Group Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/egg-group/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetGrowthRateAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Growth Rate Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/growth-rate/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        public async Task<Tuple<bool, string, string>> GetStatAsync(string nameOrId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameOrId)) return Fail("Stat Name Or ID is empty.");
                var req = NewRequest(HttpMethod.Get, "api/v2/stat/" + Esc(nameOrId.Trim().ToLowerInvariant()) + "/");
                return await SendAsync(req).ConfigureAwait(false);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Image methods

        // Sprites are hosted on raw.githubusercontent.com, not pokeapi.co — an absolute URL,
        // fetched as raw bytes rather than through the JSON SendAsync path.
        public async Task<Tuple<bool, byte[], string>> GetImageBytesAsync(string absoluteUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(absoluteUrl)) return FailBytes("Sprite URL is empty.");

                using (var req = new HttpRequestMessage(HttpMethod.Get, absoluteUrl))
                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    if ((int)res.StatusCode < 200 || (int)res.StatusCode > 299)
                        return FailBytes("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase);

                    var bytes = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    return new Tuple<bool, byte[], string>(true, bytes, "");
                }
            }
            catch (Exception ex) { return FailBytes(ex.ToString()); }
        }

        private static Tuple<bool, byte[], string> FailBytes(string error)
            => new Tuple<bool, byte[], string>(false, null, error);

        #endregion

        #region Private helpers

        private async Task<Tuple<bool, string, string>> SendAsync(HttpRequestMessage template)
        {
            string cacheKey = template.RequestUri.ToString();
            if (_responseCache.TryGetValue(cacheKey, out var cachedResult))
                return cachedResult;

            for (int attempt = 0; ; attempt++)
            {
                int status; string reason; string body; TimeSpan? retryAfter;

                using (var req = Clone(template))
                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    status     = (int)res.StatusCode;
                    reason     = res.ReasonPhrase;
                    body       = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    retryAfter = res.Headers.RetryAfter?.Delta;
                }

                if (status == 429 && attempt < 3)
                {
                    var delay = retryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    if (delay > TimeSpan.FromSeconds(30)) delay = TimeSpan.FromSeconds(30);
                    await Task.Delay(delay).ConfigureAwait(false);
                    continue;
                }

                if (status == 404)
                    return Fail("HTTP 404 — not found. Check the name or ID.\n" + body);

                if (status < 200 || status > 299)
                    return Fail("HTTP " + status + " " + reason + "\n" + body);

                var result = Ok(body);
                _responseCache[cacheKey] = result;
                return result;
            }
        }

        // No body to clone — every call is a GET, so this is a plain synchronous copy
        private static HttpRequestMessage Clone(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            foreach (var h in req.Headers) clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
            return clone;
        }

        private static Tuple<bool, string, string> Ok(string body)
            => new Tuple<bool, string, string>(true, body, "");

        private static Tuple<bool, string, string> Fail(string error)
            => new Tuple<bool, string, string>(false, "", error);

        #endregion
    }
}
