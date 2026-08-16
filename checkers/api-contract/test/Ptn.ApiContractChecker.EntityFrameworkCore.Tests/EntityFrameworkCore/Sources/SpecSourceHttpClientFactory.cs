using System.Net;
using System.Net.Http.Headers;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Sources;

// islevi: SpecSource HTTP adapterlerini gercek soket olmadan yapilandirilabilir yanit ve hatalarla calistiran client factory'dir.
// sistemdeki gorevi: Fetch ve reachability tuketicilerinin ayni isimli tasimayi, credential header'ini ve sinirli okumayi deterministik test etmesini saglar.
public class SpecSourceHttpClientFactory : IHttpClientFactory
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private byte[] _content = "{}"u8.ToArray();
    private string _mediaType = "application/json";
    private bool _includeContentLength = true;
    private long? _declaredContentLength;
    private Exception? _exception;

    // Son request'te gorulen header adini test dogrulamasi icin tutar.
    public string? ObservedHeaderName { get; private set; }

    // Son request'te gorulen header degerini test dogrulamasi icin tutar.
    public string? ObservedHeaderValue { get; private set; }

    // Son client cozumlemesinde kullanilan kararli isimdir.
    public string? ObservedClientName { get; private set; }

    // Son request'in credential icermeyen hedef adresidir.
    public Uri? ObservedRequestUri { get; private set; }

    // Son response govdesinden gercekte okunan bayt sayisidir.
    public int ContentBytesRead { get; private set; }

    // Fake'i varsayilan basarili JSON yanitina ve bos gozlemlere dondurur.
    public void Reset()
    {
        _statusCode = HttpStatusCode.OK;
        _content = "{}"u8.ToArray();
        _mediaType = "application/json";
        _includeContentLength = true;
        _declaredContentLength = null;
        _exception = null;
        ObservedHeaderName = null;
        ObservedHeaderValue = null;
        ObservedRequestUri = null;
        ContentBytesRead = 0;
    }

    // Bir sonraki request icin durum, govde, medya tipi ve Content-Length davranisini belirler.
    public void ConfigureResponse(
        HttpStatusCode statusCode,
        byte[] content,
        string mediaType,
        bool includeContentLength = true,
        long? declaredContentLength = null)
    {
        Reset();
        _statusCode = statusCode;
        _content = content;
        _mediaType = mediaType;
        _includeContentLength = includeContentLength;
        _declaredContentLength = declaredContentLength;
    }

    // Bir sonraki request'in verilen ag veya timeout hatasini firlatmasini saglar.
    public void ConfigureException(Exception exception)
    {
        Reset();
        _exception = exception;
    }

    // Her tuketiciye ayni yapilandirilabilir handler'i kullanan ayri HttpClient verir.
    public HttpClient CreateClient(string name)
    {
        ObservedClientName = name;
        return new HttpClient(new ConfigurableHandler(this));
    }

    // Request'i gozlemleyip yapilandirilan response'u stream tabanli govdeyle kurar.
    private HttpResponseMessage CreateResponse(HttpRequestMessage request)
    {
        if (_exception != null)
        {
            throw _exception;
        }

        var header = request.Headers.FirstOrDefault();
        ObservedHeaderName = header.Key;
        ObservedHeaderValue = header.Value?.SingleOrDefault();
        ObservedRequestUri = request.RequestUri;

        var content = new StreamContent(new TrackingReadStream(_content, this));
        content.Headers.ContentType = new MediaTypeHeaderValue(_mediaType);
        content.Headers.ContentLength = _includeContentLength
            ? _declaredContentLength ?? _content.Length
            : null;

        return new HttpResponseMessage(_statusCode)
        {
            Content = content
        };
    }

    // Handler ve stream'in okuma sayacini tek sahibinde toplar.
    private void RecordRead(int bytesRead)
    {
        ContentBytesRead += bytesRead;
    }

    // islevi: HTTP request'i dis aga gondermeden ortak fake response'una yonlendirir.
    // sistemdeki gorevi: Uretim IHttpClientFactory sinirini korurken transport sonucunu test tarafindan kontrol ettirir.
    private sealed class ConfigurableHandler : HttpMessageHandler
    {
        private readonly SpecSourceHttpClientFactory _owner;

        public ConfigurableHandler(SpecSourceHttpClientFactory owner)
        {
            _owner = owner;
        }

        // Request'i gozlemleyen factory'ye teslim edip response'u tamamlanmis task olarak dondurur.
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_owner.CreateResponse(request));
    }

    // islevi: Fake response govdesinden tuketicinin gercekte okudugu baytlari sayar.
    // sistemdeki gorevi: Content-Length olmasa da boyut guard'inin max + 1 baytta okumayi kestigini kanitlatir.
    private sealed class TrackingReadStream : Stream
    {
        private readonly MemoryStream _inner;
        private readonly SpecSourceHttpClientFactory _owner;

        public TrackingReadStream(byte[] content, SpecSourceHttpClientFactory owner)
        {
            _inner = new MemoryStream(content, writable: false);
            _owner = owner;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        // Senkron okumayi inner stream'e iletip tuketilen bayti kaydeder.
        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _inner.Read(buffer, offset, count);
            _owner.RecordRead(bytesRead);
            return bytesRead;
        }

        // Modern async okumayi inner stream'e iletip tuketilen bayti kaydeder.
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var bytesRead = await _inner.ReadAsync(buffer, cancellationToken);
            _owner.RecordRead(bytesRead);
            return bytesRead;
        }

        // Buffer tabanli async okumayi ortak sayacli modern metoda yonlendirir.
        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
            => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        // Stream konumlandirmayi desteklemedigi icin acikca reddeder.
        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        // Salt-okunur stream uzunluk degisimini desteklemez.
        public override void SetLength(long value)
            => throw new NotSupportedException();

        // Salt-okunur stream yazmayi desteklemez.
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        // Bellek stream'inde flush edilecek cikis olmadigindan islem yapmaz.
        public override void Flush()
        {
        }

        // Response sonlandiginda inner stream'i de serbest birakir.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
