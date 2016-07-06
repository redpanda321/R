#region License
/*
 * WebSocket.cs
 *
 * A C# implementation of the WebSocket interface.
 *
 * This code is derived from WebSocket.java
 * (http://github.com/adamac/Java-WebSocket-client).
 *
 * The MIT License
 *
 * Copyright (c) 2009 Adam MacBeth
 * Copyright (c) 2010-2014 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WebSocketSharp.Net;


namespace WebSocketSharp
{
  /// <summary>
  /// Implements the WebSocket interface.
  /// </summary>
  /// <remarks>
  /// The WebSocket class provides a set of methods and properties for two-way
  /// communication using the WebSocket protocol
  /// (<see href="http://tools.ietf.org/html/rfc6455">RFC 6455</see>).
  /// </remarks>
  public class WebSocket : IDisposable
  {
    #region Private Const Fields

    private const string _guid    = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    private const string _version = "13";

    #endregion

    #region Private Fields

    private string                  _base64Key;
    private RemoteCertificateValidationCallback
                                    _certValidationCallback;
    private CompressionMethod       _compression;
    private CookieCollection        _cookies;
    private Func<CookieCollection, CookieCollection, bool>
                                    _cookiesValidation;
    private string                  _extensions;
    private AutoResetEvent          _exitReceiving;
    private object                  _forConn;
    private object                  _forSend;
    private volatile Logger         _logger;
    private string                  _origin;
    private Timer                   _pingSender;
    private long                    _pingSentAt;
    private int                     _pingInterval;
    private string                  _protocol;
    private string []               _protocols;
    private volatile WebSocketState _readyState;
    private long                    _rtt;
    private volatile int            _lastReceiveTime;
    private bool                    _secure;
    private WsStream                _stream;
    private TcpClient               _tcpClient;
    private int                     _timeoutRtt;
    private Uri                     _uri;
    private NameValueCollection     _customHeaders;

    #endregion

    #region Internal Const Fields

    internal const int FragmentLength = 1016; // Max value is int.MaxValue - 14.

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocket"/> class with the
    /// specified WebSocket URL and subprotocols.
    /// </summary>
    /// <param name="url">
    /// A <see cref="string"/> that represents the WebSocket URL to connect.
    /// </param>
    /// <param name="protocols">
    /// An array of <see cref="string"/> that contains the WebSocket subprotocols
    /// if any. Each value of <paramref name="protocols"/> must be a token defined
    /// in <see href="http://tools.ietf.org/html/rfc2616#section-2.2">RFC 2616</see>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="url"/> is invalid.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="protocols"/> is invalid.
    ///   </para>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="url"/> is <see langword="null"/>.
    /// </exception>
    public WebSocket (string url, params string [] protocols) : this (url, null, protocols)
    {
    }

    public WebSocket (string url, Logger logger, params string [] protocols)
    {
      if (url == null)
        throw new ArgumentNullException ("url");

      string msg;
      if (!url.TryCreateWebSocketUri (out _uri, out msg))
        throw new ArgumentException (msg, "url");

      if (protocols != null && protocols.Length > 0) {
        msg = protocols.CheckIfValidProtocols ();
        if (msg != null)
          throw new ArgumentException (msg, "protocols");

        _protocols = protocols;
      }

      _base64Key = CreateBase64Key ();
      _logger = logger ?? new Logger ();
      _secure = _uri.Scheme == "wss";
      _pingInterval = 5000;
      _timeoutRtt = 3000;
      _lastReceiveTime = (int)(DateTime.UtcNow.Ticks / 10000);
      init ();
    }

    #endregion

    #region Internal Properties

    internal Func<CookieCollection, CookieCollection, bool> CookiesValidation {
      get {
        return _cookiesValidation;
      }

      set {
        _cookiesValidation = value;
      }
    }

    internal bool IsConnected {
      get {
        return _readyState == WebSocketState.OPEN ||
               _readyState == WebSocketState.CLOSING;
      }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the compression method used to compress the message.
    /// </summary>
    /// <value>
    /// One of the <see cref="CompressionMethod"/> enum values, indicates the
    /// compression method used to compress the message. The default value is
    /// <see cref="CompressionMethod.NONE"/>.
    /// </value>
    public CompressionMethod Compression {
      get {
        return _compression;
      }

      set {
        lock (_forConn) {
          var msg = checkIfAvailable (
            "Set operation of Compression", false, false);

          if (msg != null) {
            _logger.Error (msg);
            error (msg);

            return;
          }

          _compression = value;
        }
      }
    }

    /// <summary>
    /// Gets the HTTP cookies used in the WebSocket connection request and
    /// response.
    /// </summary>
    /// <value>
    /// An IEnumerable&lt;Cookie&gt; interface that provides an enumerator which
    /// supports the iteration over the collection of cookies.
    /// </value>
    public IEnumerable<Cookie> Cookies {
      get {
        lock (_cookies.SyncRoot) {
          foreach (Cookie cookie in _cookies)
            yield return cookie;
        }
      }
    }

    /// <summary>
    /// Gets the WebSocket extensions selected by the server.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> that represents the extensions if any.
    /// The default value is <see cref="String.Empty"/>.
    /// </value>
    public string Extensions {
      get {
        return _extensions;
      }
    }

    /// <summary>
    /// Gets a value that represents elapsed time since the connection last received in milliseconds.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> that represents elapsed time since last received in milliseconds. (the connection received even once)
    /// An <see cref="int"/> that represents elapsed time since instance created in milliseconds. (otherwise)
    /// </value>
    public int SinceLastReceiveMS {
      get {
          return (int)(DateTime.UtcNow.Ticks / 10000) - _lastReceiveTime;
      }
    }


    /// <summary>
    /// Gets a value indicating whether the WebSocket connection is alive.
    /// </summary>
    /// <value>
    /// <c>true</c> if the connection is alive; otherwise, <c>false</c>.
    /// </value>
    public bool IsAlive {
      get {
        if (_readyState != WebSocketState.OPEN)
          return false;
        if (_pingSentAt == 0)
          return true;
        long elapse = DateTime.Now.Ticks / 10000 - _pingSentAt;
        if (elapse > _timeoutRtt)
          return false;
        return true;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the WebSocket connection is secure.
    /// </summary>
    /// <value>
    /// <c>true</c> if the connection is secure; otherwise, <c>false</c>.
    /// </value>
    public bool IsSecure {
      get {
        return _secure;
      }
    }

    /// <summary>
    /// Gets the logging functions.
    /// </summary>
    /// <remarks>
    /// The default logging level is <see cref="LogLevel.ERROR"/>. If you would
    /// like to change it, you should set the <c>Log.Level</c> property to any of
    /// the <see cref="LogLevel"/> enum values.
    /// </remarks>
    /// <value>
    /// A <see cref="Logger"/> that provides the logging functions.
    /// </value>
    public Logger Log {
      get {
        return _logger;
      }

      internal set {
        _logger = value;
      }
    }

    /// <summary>
    /// Gets or sets the value of the Origin header to send with the WebSocket
    /// connection request to the server.
    /// </summary>
    /// <remarks>
    /// The <see cref="WebSocket"/> sends the Origin header if this property has
    /// any.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   A <see cref="string"/> that represents the value of the
    ///   <see href="http://tools.ietf.org/html/rfc6454#section-7">HTTP Origin
    ///   header</see> to send. The default value is <see langword="null"/>.
    ///   </para>
    ///   <para>
    ///   The Origin header has the following syntax:
    ///   <c>&lt;scheme&gt;://&lt;host&gt;[:&lt;port&gt;]</c>
    ///   </para>
    /// </value>
    public string Origin {
      get {
        return _origin;
      }

      set {
        lock (_forConn) {
          var msg = checkIfAvailable ("Set operation of Origin", false, false);
          if (msg == null) {
            if (value.IsNullOrEmpty ()) {
              _origin = value;
              return;
            }

            Uri origin;
            if (!Uri.TryCreate (value, UriKind.Absolute, out origin) ||
                origin.Segments.Length > 1)
              msg = "The syntax of Origin must be '<scheme>://<host>[:<port>]'.";
          }

          if (msg != null) {
            _logger.Error (msg);
            error (msg);

            return;
          }

          _origin = value.TrimEnd ('/');
        }
      }
    }

    /// <summary>
    /// Gets the WebSocket subprotocol selected by the server.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> that represents the subprotocol if any.
    /// The default value is <see cref="String.Empty"/>.
    /// </value>
    public string Protocol {
      get {
        return _protocol;
      }
    }

    /// <summary>
    /// Gets the state of the WebSocket connection.
    /// </summary>
    /// <value>
    /// One of the <see cref="WebSocketState"/> enum values, indicates the state
    /// of the WebSocket connection.
    /// The default value is <see cref="WebSocketState.CONNECTING"/>.
    /// </value>
    public WebSocketState ReadyState {
      get {
        return _readyState;
      }
    }

    /// <summary>
    /// Gets or sets the callback used to validate the certificate supplied by
    /// the server.
    /// </summary>
    /// <remarks>
    /// If the value of this property is <see langword="null"/>, the validation
    /// does nothing with the server certificate, always returns valid.
    /// </remarks>
    /// <value>
    /// A <see cref="RemoteCertificateValidationCallback"/> delegate that
    /// references the method(s) used to validate the server certificate.
    /// The default value is <see langword="null"/>.
    /// </value>
    public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
      get {
        return _certValidationCallback;
      }

      set {
        lock (_forConn) {
          var msg = checkIfAvailable (
            "Set operation of ServerCertificateValidationCallback", false, false);

          if (msg != null) {
            _logger.Error (msg);
            error (msg);

            return;
          }

          _certValidationCallback = value;
        }
      }
    }

    /// <summary>
    /// Gets the WebSocket URL to connect.
    /// </summary>
    /// <value>
    /// A <see cref="Uri"/> that represents the WebSocket URL to connect.
    /// </value>
    public Uri Url {
      get {
        return _uri;
      }

      internal set {
        _uri = value;
      }
    }

    /// <summary>
    /// Gets the WebSocket RTT.
    /// </summary>
    /// <value>
    /// A <see cref="long"/> that represents the RTT in millisecond.
    /// The default value is 0.
    /// </value>
    public long Rtt {
      get {
        return _rtt;
      }
    }

    /// <summary>
    /// Gets and Sets the WebSocket PING interval.
    /// </summary>
    /// <value>
    /// A <see cref="int"/> that represents the Websocket PING interval in millisecond.
    /// The default value is 5000.
    /// </value>
    public int PingInterval {
      get {
        return _pingInterval;
      }
      set {
        _pingInterval = value;
      }
    }

    /// <summary>
    /// Gets and Sets the WebSocket timeout RTT.
    /// </summary>
    /// <value>
    /// A <see cref="int"/> that represents RTT the Websocket is regarded as timeout in millisecond.
    /// The default value is 3000.
    /// </value>
    public int TimeoutRtt {
      get {
        return _timeoutRtt;
      }
      set {
        _timeoutRtt = value;
      }
    }

    #endregion

    #region Public Events

    /// <summary>
    /// Occurs when the WebSocket connection has been closed.
    /// </summary>
    public EventHandler<CloseEventArgs> OnClose;

    /// <summary>
    /// Occurs when the <see cref="WebSocket"/> gets an error.
    /// </summary>
    public EventHandler<ErrorEventArgs> OnError;

    /// <summary>
    /// Occurs when the <see cref="WebSocket"/> receives a data frame.
    /// </summary>
    public EventHandler<MessageEventArgs> OnMessage;

    /// <summary>
    /// Occurs when the WebSocket connection has been established.
    /// </summary>
    public EventHandler OnOpen;

    #endregion

    #region Private Methods

    private bool acceptCloseFrame (WsFrame frame)
    {
      var payload = frame.PayloadData;
      close (payload, !payload.ContainsReservedCloseStatusCode, false);

      return false;
    }

    private bool acceptDataFrame (WsFrame frame)
    {
      var args = frame.IsCompressed
               ? new MessageEventArgs (
                   frame.Opcode,
                   frame.PayloadData.ApplicationData.Decompress (_compression))
               : new MessageEventArgs (frame.Opcode, frame.PayloadData);

      OnMessage.Emit (this, args);
      return true;
    }

    private void acceptException (Exception exception, string reason)
    {
      var code = CloseStatusCode.ABNORMAL;
      var msg = reason;
      if (exception is WebSocketException) {
        var wsex = (WebSocketException) exception;
        code = wsex.Code;
        reason = wsex.Message;
      }

      if (code == CloseStatusCode.ABNORMAL ||
          code == CloseStatusCode.TLS_HANDSHAKE_FAILURE) {
        _logger.Fatal (exception.ToString ());
        reason = msg;
      }
      else {
        _logger.Error (reason);
        msg = null;
      }

      error (msg ?? code.GetMessage (), exception);
      close (code, reason ?? code.GetMessage (), false);
    }

    private bool acceptFragmentedFrame (WsFrame frame)
    {
      return frame.IsContinuation // Not first fragment
             ? true
             : acceptFragments (frame);
    }

    private bool acceptFragments (WsFrame first)
    {
      using (var concatenated = new MemoryStream ()) {
        concatenated.WriteBytes (first.PayloadData.ApplicationData);
        if (!concatenateFragmentsInto (concatenated))
          return false;

        byte [] data;
        if (_compression != CompressionMethod.NONE) {
          data = concatenated.DecompressToArray (_compression);
        }
        else {
          concatenated.Close ();
          data = concatenated.ToArray ();
        }

        OnMessage.Emit (this, new MessageEventArgs (first.Opcode, data));
        return true;
      }
    }

    private bool acceptFrame (WsFrame frame)
    {
      _lastReceiveTime = (int)(DateTime.UtcNow.Ticks / 10000);
      return frame.IsCompressed && _compression == CompressionMethod.NONE
             ? acceptUnsupportedFrame (
                 frame,
                 CloseStatusCode.INCORRECT_DATA,
                 "A compressed data has been received without available decompression method.")
             : frame.IsFragmented
               ? acceptFragmentedFrame (frame)
               : frame.IsData
                 ? acceptDataFrame (frame)
                 : frame.IsPing
                   ? acceptPingFrame (frame)
                   : frame.IsPong
                     ? acceptPongFrame (frame)
                     : frame.IsClose
                       ? acceptCloseFrame (frame)
                       : acceptUnsupportedFrame (frame, CloseStatusCode.POLICY_VIOLATION, null);
    }

    private bool acceptPingFrame (WsFrame frame)
    {
      var mask = Mask.MASK;
      if (send (WsFrame.CreatePongFrame (mask, frame.PayloadData)))
        _logger.Trace ("Returned a Pong.");

      return true;
    }

    private bool acceptPongFrame (WsFrame frame)
    {
      _logger.Trace ("Received a Pong.");

      _rtt = DateTime.Now.Ticks / 10000 - _pingSentAt;

      _pingSentAt = 0;
      if (_pingSender != null)
        _pingSender.Dispose ();

      _pingSender = new Timer((object o) => {
        Ping ();
      }, null, _pingInterval, Timeout.Infinite);

      return true;
    }

    private bool acceptUnsupportedFrame (
      WsFrame frame, CloseStatusCode code, string reason)
    {
      _logger.Debug ("Unsupported frame:\n" + frame.PrintToString (false));
      acceptException (new WebSocketException (code, reason), null);

      return false;
    }

    private string checkIfAvailable (
      string operation, bool availableAsServer, bool availableAsConnected)
    {
      return !availableAsConnected
               ? _readyState.CheckIfConnectable ()
               : null;
    }

    private string checkIfCanClose (Func<string> checkParams)
    {
      return _readyState.CheckIfClosable () ?? checkParams ();
    }

    private string checkIfCanConnect ()
    {
      return _readyState.CheckIfConnectable ();
    }

    private string checkIfCanSend (Func<string> checkParams)
    {
      return _readyState.CheckIfOpen () ?? checkParams ();
    }

    // As client
    private string checkIfValidHandshakeResponse (HandshakeResponse response)
    {
      var headers = response.Headers;
      return response.IsUnauthorized
             ? "HTTP authorization is required."
             : !response.IsWebSocketResponse
               ? "Not WebSocket connection response."
               : !validateSecWebSocketAcceptHeader (
                   headers ["Sec-WebSocket-Accept"])
                 ? "Invalid Sec-WebSocket-Accept header."
                 : !validateSecWebSocketProtocolHeader (
                     headers ["Sec-WebSocket-Protocol"])
                   ? "Invalid Sec-WebSocket-Protocol header."
                   : !validateSecWebSocketVersionHeader (
                       headers ["Sec-WebSocket-Version"])
                     ? "Invalid Sec-WebSocket-Version header."
                     : null;
    }

    private void close (CloseStatusCode code, string reason, bool wait)
    {
      close (
        new PayloadData (((ushort) code).Append (reason)),
        !code.IsReserved (),
        wait);
    }

    private void close (PayloadData payload, bool send, bool wait)
    {
      lock (_forConn) {
        if (_readyState == WebSocketState.CLOSING ||
            _readyState == WebSocketState.CLOSED) {
          _logger.Info (
            "Closing the WebSocket connection has already been done.");
          return;
        }

        _readyState = WebSocketState.CLOSING;
      }

      _logger.Trace ("Start closing handshake.");

      var args = new CloseEventArgs (payload);
      args.WasClean =
        closeHandshake (
        send ? WsFrame.CreateCloseFrame (Mask.MASK, payload).ToByteArray ()
                 : null,
        wait ? 5000 : 0,
        closeClientResources);

      _logger.Trace ("End closing handshake.");

      _readyState = WebSocketState.CLOSED;
      try {
        if (_pingSender != null)
          _pingSender.Dispose ();
        OnClose.Emit (this, args);
      }
      catch (Exception ex) {
        _logger.Fatal (ex.ToString ());
        error ("An exception has occurred while OnClose.", ex);
      }
    }

    private void closeAsync (PayloadData payload, bool send, bool wait)
    {
      Action<PayloadData, bool, bool> closer = close;
      closer.BeginInvoke (
        payload, send, wait, ar => closer.EndInvoke (ar), null);
    }

    // As client
    private void closeClientResources ()
    {
      if (_stream != null) {
        _stream.Dispose ();
        _stream = null;
      }

      if (_tcpClient != null) {
        _tcpClient.Close ();
        _tcpClient = null;
      }
    }

    private bool closeHandshake (byte [] frameAsBytes, int timeOut, Action release)
    {
      var sent = frameAsBytes != null && _stream.Write (frameAsBytes);
      var received = timeOut == 0 ||
                     (sent && _exitReceiving != null && _exitReceiving.WaitOne (timeOut));

      release ();

      if (_exitReceiving != null) {
        _exitReceiving.Close ();
        _exitReceiving = null;
      }

      var result = sent && received;
      _logger.Debug (
        String.Format (
          "Was clean?: {0}\nsent: {1} received: {2}", result, sent, received));

      return result;
    }

    private bool concatenateFragmentsInto (Stream dest)
    {
      var frame = _stream.ReadFrame ();

      // MORE & CONT
      if (!frame.IsFinal && frame.IsContinuation) {
        dest.WriteBytes (frame.PayloadData.ApplicationData);
        return concatenateFragmentsInto (dest);
      }

      // FINAL & CONT
      if (frame.IsFinal && frame.IsContinuation) {
        dest.WriteBytes (frame.PayloadData.ApplicationData);
        return true;
      }

      // FINAL & PING
      if (frame.IsFinal && frame.IsPing) {
        acceptPingFrame (frame);
        return concatenateFragmentsInto (dest);
      }

      // FINAL & PONG
      if (frame.IsFinal && frame.IsPong) {
        acceptPongFrame (frame);
        return concatenateFragmentsInto (dest);
      }

      // FINAL & CLOSE
      if (frame.IsFinal && frame.IsClose)
        return acceptCloseFrame (frame);

      // ?
      return acceptUnsupportedFrame (
        frame, CloseStatusCode.INCORRECT_DATA, null);
    }

    private bool connect ()
    {
      lock (_forConn) {
        var msg = _readyState.CheckIfConnectable ();
        if (msg != null) {
          _logger.Error (msg);
          error (msg);

          return false;
        }

        try {
          if (doHandshake ()) {
            _readyState = WebSocketState.OPEN;
            return true;
          }
        }
        catch (Exception ex) {
          acceptException (
            ex, "An exception has occurred while connecting.");
        }

        return false;
      }
    }

    // As client
    private string createExtensionsRequest ()
    {
      var extensions = new StringBuilder (64);
      if (_compression != CompressionMethod.NONE)
        extensions.Append (_compression.ToCompressionExtension ());

      return extensions.Length > 0
             ? extensions.ToString ()
             : String.Empty;
    }

    // As client
    private HandshakeRequest createHandshakeRequest ()
    {
      var path = _uri.PathAndQuery;
      var host = _uri.Port == 80
               ? _uri.DnsSafeHost
               : _uri.Authority;

      var req = new HandshakeRequest (path);
      var headers = req.Headers;

      headers ["Host"] = host;

      if (!_origin.IsNullOrEmpty ())
        headers ["Origin"] = _origin;

      headers ["Sec-WebSocket-Key"] = _base64Key;

      if (_protocols != null)
        headers ["Sec-WebSocket-Protocol"] = _protocols.ToString (", ");

      var extensions = createExtensionsRequest ();
      if (extensions.Length > 0)
        headers ["Sec-WebSocket-Extensions"] = extensions;

      headers ["Sec-WebSocket-Version"] = _version;

      if (_cookies.Count > 0)
        req.SetCookies (_cookies);

      foreach (var key in _customHeaders.AllKeys){
        headers [key] = _customHeaders [key];
      }

      return req;
    }

    // As server
    private HandshakeResponse createHandshakeResponse ()
    {
      var res = new HandshakeResponse (HttpStatusCode.SwitchingProtocols);
      var headers = res.Headers;

      headers ["Sec-WebSocket-Accept"] = CreateResponseKey (_base64Key);

      if (_protocol.Length > 0)
        headers ["Sec-WebSocket-Protocol"] = _protocol;

      if (_extensions.Length > 0)
        headers ["Sec-WebSocket-Extensions"] = _extensions;

      if (_cookies.Count > 0)
        res.SetCookies (_cookies);

      foreach (var key in _customHeaders.AllKeys){
        headers [key] = _customHeaders [key];
      }

      return res;
    }

    // As server
    private HandshakeResponse createHandshakeResponse (HttpStatusCode code)
    {
      var res = HandshakeResponse.CreateCloseResponse (code);
      res.Headers ["Sec-WebSocket-Version"] = _version;

      return res;
    }

    // As client
    private bool doHandshake ()
    {
      setClientStream ();
      var res = sendHandshakeRequest ();
      var msg = checkIfValidHandshakeResponse (res);
      if (msg != null) {
        _logger.Error (msg);

        msg = "An error has occurred while connecting.";
        error (msg);
        close (CloseStatusCode.ABNORMAL, msg, false);

        return false;
      }

      processRespondedExtensions (res.Headers ["Sec-WebSocket-Extensions"]);

      var cookies = res.Cookies;
      if (cookies.Count > 0)
        _cookies.SetOrRemove (cookies);

      return true;
    }

    private void error (string message, Exception exc = null)
    {
      OnError.Emit (this, new ErrorEventArgs (message, exc));
    }

    private void init ()
    {
      _compression = CompressionMethod.NONE;
      _cookies = new CookieCollection ();
      _extensions = String.Empty;
      _forConn = new object ();
      _forSend = new object ();
      _protocol = String.Empty;
      _readyState = WebSocketState.CONNECTING;
      _customHeaders = new NameValueCollection();
    }

    private void open ()
    {
      try {
        OnOpen.Emit (this, EventArgs.Empty);
        if (_readyState == WebSocketState.OPEN) {
          startReceiving ();
          Ping ();
        }
      }
      catch (Exception ex) {
        acceptException (
          ex, "An exception has occurred while opening.");
      }
    }

    // As server
    private void processRequestedExtensions (string extensions)
    {
      var comp = false;
      var buffer = new List<string> ();
      foreach (var e in extensions.SplitHeaderValue (',')) {
        var extension = e.Trim ();
        var tmp = extension.RemovePrefix ("x-webkit-");
        if (!comp && tmp.IsCompressionExtension ()) {
          var method = tmp.ToCompressionMethod ();
          if (method != CompressionMethod.NONE) {
            _compression = method;
            comp = true;
            buffer.Add (extension);
          }
        }
      }

      if (buffer.Count > 0)
        _extensions = buffer.ToArray ().ToString (", ");
    }

    // As client
    private void processRespondedExtensions (string extensions)
    {
      var comp = _compression != CompressionMethod.NONE ? true : false;
      var hasComp = false;
      if (extensions != null && extensions.Length > 0) {
        foreach (var e in extensions.SplitHeaderValue (',')) {
          var extension = e.Trim ();
          if (comp && !hasComp && extension.Equals (_compression))
            hasComp = true;
        }

        _extensions = extensions;
      }

      if (comp && !hasComp)
        _compression = CompressionMethod.NONE;
    }

    // As client
    private HandshakeResponse receiveHandshakeResponse ()
    {
      var res = _stream.ReadHandshakeResponse ();
      _logger.Debug (
        "A response to this WebSocket connection request:\n" + res.ToString ());

      return res;
    }

    private bool send (byte [] frame)
    {
      lock (_forConn) {
        if (_readyState != WebSocketState.OPEN) {
          var msg = "The WebSocket connection isn't available.";
          _logger.Error (msg);
          error (msg);

          return false;
        }

        return _stream.Write (frame);
      }
    }

    // As client
    private void send (HandshakeRequest request)
    {
      _logger.Debug (
        String.Format (
          "A WebSocket connection request to {0}:\n{1}", _uri, request));

      _stream.WriteHandshake (request);
    }

    // As server
    private bool send (HandshakeResponse response)
    {
      _logger.Debug (
        "A response to the WebSocket connection request:\n" + response.ToString ());

      return _stream.WriteHandshake (response);
    }

    private bool send (WsFrame frame)
    {
      lock (_forConn) {
        if (_readyState != WebSocketState.OPEN) {
          var msg = "The WebSocket connection isn't available.";
          _logger.Error (msg);
          error (msg);

          return false;
        }

        return _stream.Write (frame.ToByteArray ());
      }
    }

    private bool send (Opcode opcode, byte [] data)
    {
      lock (_forSend) {
        var sent = false;
        try {
          var compressed = false;
          if (_compression != CompressionMethod.NONE) {
            data = data.Compress (_compression);
            compressed = true;
          }

          var mask = Mask.MASK;
          sent = send (
            WsFrame.CreateFrame (Fin.FINAL, opcode, mask, data, compressed));
        }
        catch (Exception ex) {
          _logger.Fatal (ex.ToString ());
          error ("An exception has occurred while sending a data.", ex);
        }

        return sent;
      }
    }

    private bool send (Opcode opcode, Stream stream)
    {
      lock (_forSend) {
        var sent = false;
        var src = stream;
        var compressed = false;
        try {
          if (_compression != CompressionMethod.NONE) {
            stream = stream.Compress (_compression);
            compressed = true;
          }

          var mask = Mask.MASK;
          sent = sendFragmented (opcode, stream, mask, compressed);
        }
        catch (Exception ex) {
          _logger.Fatal (ex.ToString ());
          error ("An exception has occurred while sending a data.", ex);
        }
        finally {
          if (compressed)
            stream.Dispose ();

          src.Dispose ();
        }

        return sent;
      }
    }

    private void sendAsync (Opcode opcode, byte [] data, Action<bool> completed)
    {
      bool sent = send (opcode, data);
      if (completed != null) {
        completed(sent);
      }
    }

    private void sendAsync (Opcode opcode, Stream stream, Action<bool> completed)
    {
      bool sent = send (opcode, stream);
      if (completed != null) {
          completed(sent);
      }
    }

    private bool sendFragmented (
      Opcode opcode, Stream stream, Mask mask, bool compressed)
    {
      var len = stream.Length;
      if (sendFragmented (opcode, stream, len, mask, compressed) == len)
        return true;

      var msg = "Sending fragmented data is interrupted.";
      _logger.Error (msg);
      error (msg);
      close (CloseStatusCode.ABNORMAL, msg, false);

      return false;
    }

    private long sendFragmented (
      Opcode opcode, Stream stream, long length, Mask mask, bool compressed)
    {
      var quo = length / FragmentLength;
      var rem = (int) (length % FragmentLength);
      var count = rem == 0 ? quo - 2 : quo - 1;

      long sentLen = 0;
      int readLen = 0;
      byte [] buffer = null;

      // Not fragment
      if (quo == 0) {
        buffer = new byte [rem];
        readLen = stream.Read (buffer, 0, rem);
        if (readLen == rem &&
            send (
              WsFrame.CreateFrame (Fin.FINAL, opcode, mask, buffer, compressed)))
          sentLen = readLen;

        return sentLen;
      }

      buffer = new byte [FragmentLength];

      // First
      readLen = stream.Read (buffer, 0, FragmentLength);
      if (readLen == FragmentLength &&
          send (
            WsFrame.CreateFrame (Fin.MORE, opcode, mask, buffer, compressed)))
        sentLen = readLen;
      else
        return sentLen;

      // Mid
      for (long i = 0; i < count; i++) {
        readLen = stream.Read (buffer, 0, FragmentLength);
        if (readLen == FragmentLength &&
            send (
              WsFrame.CreateFrame (Fin.MORE, Opcode.CONT, mask, buffer, compressed)))
          sentLen += readLen;
        else
          return sentLen;
      }

      // Final
      var tmpLen = FragmentLength;
      if (rem != 0)
        buffer = new byte [tmpLen = rem];

      readLen = stream.Read (buffer, 0, tmpLen);
      if (readLen == tmpLen &&
          send (
            WsFrame.CreateFrame (Fin.FINAL, Opcode.CONT, mask, buffer, compressed)))
        sentLen += readLen;

      return sentLen;
    }

    // As client
    private HandshakeResponse sendHandshakeRequest ()
    {
      var req = createHandshakeRequest ();
      var res = sendHandshakeRequest (req);
      return res;
    }

    // As client
    private HandshakeResponse sendHandshakeRequest (HandshakeRequest request)
    {
      send (request);
      return receiveHandshakeResponse ();
    }

    // As client
    private void setClientStream ()
    {
      var host = _uri.DnsSafeHost;
      var port = _uri.Port;
      _tcpClient = new TcpClient (host, port);
      _tcpClient.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
      _tcpClient.Client.SetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
      _stream = WsStream.CreateClientStream (
        _tcpClient, _secure, host, _certValidationCallback);
    }

    private void startReceiving ()
    {
      _exitReceiving = new AutoResetEvent (false);

      Action receive = null;
      receive = () => _stream.ReadFrameAsync (
        frame => {
          if (acceptFrame (frame))
            receive ();
          else if (_exitReceiving != null)
            _exitReceiving.Set ();
        },
        ex => acceptException (
          ex, "An exception has occurred while receiving a message."));

      receive ();
    }

    // As server
    private bool validateCookies (
      CookieCollection request, CookieCollection response)
    {
      return _cookiesValidation != null
             ? _cookiesValidation (request, response)
             : true;
    }

    // As server
    private bool validateHostHeader (string value)
    {
      if (value == null || value.Length == 0)
        return false;

      if (!_uri.IsAbsoluteUri)
        return true;

      var i = value.IndexOf (':');
      var host = i > 0 ? value.Substring (0, i) : value;
      var expected = _uri.DnsSafeHost;

      return Uri.CheckHostName (host) != UriHostNameType.Dns ||
             Uri.CheckHostName (expected) != UriHostNameType.Dns ||
             host == expected;
    }

    // As client
    private bool validateSecWebSocketAcceptHeader (string value)
    {
      return value != null && value == CreateResponseKey (_base64Key);
    }

    // As client
    private bool validateSecWebSocketProtocolHeader (string value)
    {
      if (value == null)
        return _protocols == null;

      if (_protocols == null ||
          !_protocols.Contains (protocol => protocol == value))
        return false;

      _protocol = value;
      return true;
    }

    // As client
    private bool validateSecWebSocketVersionHeader (string value)
    {
      return value == null || value == _version;
    }

    #endregion

    #region Internal Methods

    // As client
    internal static string CreateBase64Key ()
    {
      var src = new byte [16];
      var rand = new Random ();
      rand.NextBytes (src);

      return Convert.ToBase64String (src);
    }

    internal static string CreateResponseKey (string base64Key)
    {
      var buffer = new StringBuilder (base64Key, 64);
      buffer.Append (_guid);
      SHA1 sha1 = new SHA1CryptoServiceProvider ();
      var src = sha1.ComputeHash (Encoding.UTF8.GetBytes (buffer.ToString ()));

      return Convert.ToBase64String (src);
    }

    internal bool Ping ()
    {
      _pingSentAt = DateTime.Now.Ticks / 10000; // Millseconds
      return send (WsFrame.CreatePingFrame (Mask.MASK).ToByteArray ());
    }

    // As server, used to broadcast
    internal void Send (
      Opcode opcode, byte [] data, Dictionary<CompressionMethod, byte []> cache)
    {
      lock (_forSend) {
        try {
          byte [] cached;
          if (!cache.TryGetValue (_compression, out cached)) {
            cached = WsFrame.CreateFrame (
              Fin.FINAL,
              opcode,
              Mask.UNMASK,
              data.Compress (_compression),
              _compression != CompressionMethod.NONE).ToByteArray ();

            cache.Add (_compression, cached);
          }

          send (cached);
        }
        catch (Exception ex) {
          _logger.Fatal (ex.ToString ());
          error ("An exception has occurred while sending a data.", ex);
        }
      }
    }

    // As server, used to broadcast
    internal void Send (
      Opcode opcode, Stream stream, Dictionary <CompressionMethod, Stream> cache)
    {
      lock (_forSend) {
        try {
          Stream cached;
          if (!cache.TryGetValue (_compression, out cached)) {
            cached = stream.Compress (_compression);
            cache.Add (_compression, cached);
          }
          else
            cached.Position = 0;

          sendFragmented (
            opcode, cached, Mask.UNMASK, _compression != CompressionMethod.NONE);
        }
        catch (Exception ex) {
          _logger.Fatal (ex.ToString ());
          error ("An exception has occurred while sending a data.", ex);
        }
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Closes the WebSocket connection, and releases all associated resources.
    /// </summary>
    public void Close ()
    {
      var msg = _readyState.CheckIfClosable ();
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      var send = _readyState == WebSocketState.OPEN;
      close (new PayloadData (), send, send);
    }

    /// <summary>
    /// Closes the WebSocket connection with the specified <see cref="ushort"/>,
    /// and releases all associated resources.
    /// </summary>
    /// <remarks>
    /// This method emits a <see cref="OnError"/> event if <paramref name="code"/>
    /// isn't in the allowable range of the WebSocket close status code.
    /// </remarks>
    /// <param name="code">
    /// A <see cref="ushort"/> that represents the status code that indicates the
    /// reason for closure.
    /// </param>
    public void Close (ushort code)
    {
      Close (code, null);
    }

    /// <summary>
    /// Closes the WebSocket connection with the specified <see cref="CloseStatusCode"/>,
    /// and releases all associated resources.
    /// </summary>
    /// <param name="code">
    /// One of the <see cref="CloseStatusCode"/> enum values, represents the status
    /// code that indicates the reason for closure.
    /// </param>
    public void Close (CloseStatusCode code)
    {
      Close (code, null);
    }

    /// <summary>
    /// Closes the WebSocket connection with the specified <see cref="ushort"/>
    /// and <see cref="string"/>, and releases all associated resources.
    /// </summary>
    /// <remarks>
    /// This method emits a <see cref="OnError"/> event if <paramref name="code"/>
    /// isn't in the allowable range of the WebSocket close status code or the
    /// size of <paramref name="reason"/> is greater than 123 bytes.
    /// </remarks>
    /// <param name="code">
    /// A <see cref="ushort"/> that represents the status code that indicates the
    /// reason for closure.
    /// </param>
    /// <param name="reason">
    /// A <see cref="string"/> that represents the reason for closure.
    /// </param>
    public void Close (ushort code, string reason)
    {
      byte [] data = null;
      var msg = checkIfCanClose (
        () => code.CheckIfValidCloseStatusCode () ??
              (data = code.Append (reason)).CheckIfValidControlData ("reason"));

      if (msg != null) {
        _logger.Error (
          String.Format ("{0}\ncode: {1} reason: {2}", msg, code, reason));

        error (msg);
        return;
      }

      var send = _readyState == WebSocketState.OPEN && !code.IsReserved ();
      close (new PayloadData (data), send, send);
    }

    /// <summary>
    /// Closes the WebSocket connection with the specified <see cref="CloseStatusCode"/>
    /// and <see cref="string"/>, and releases all associated resources.
    /// </summary>
    /// <remarks>
    /// This method emits a <see cref="OnError"/> event if the size of
    /// <paramref name="reason"/> is greater than 123 bytes.
    /// </remarks>
    /// <param name="code">
    /// One of the <see cref="CloseStatusCode"/> enum values, represents the
    /// status code that indicates the reason for closure.
    /// </param>
    /// <param name="reason">
    /// A <see cref="string"/> that represents the reason for closure.
    /// </param>
    public void Close (CloseStatusCode code, string reason)
    {
      byte [] data = null;
      var msg = checkIfCanClose (
        () => (data = ((ushort) code).Append (reason))
              .CheckIfValidControlData ("reason"));

      if (msg != null) {
        _logger.Error (
          String.Format ("{0}\ncode: {1} reason: {2}", msg, code, reason));

        error (msg);
        return;
      }

      var send = _readyState == WebSocketState.OPEN && !code.IsReserved ();
      close (new PayloadData (data), send, send);
    }

    /// <summary>
    /// Closes the WebSocket connection asynchronously, and releases all
    /// associated resources.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the close to be complete.
    /// </remarks>
    public void CloseAsync ()
    {
      var msg = _readyState.CheckIfClosable ();
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      var send = _readyState == WebSocketState.OPEN;
      closeAsync (new PayloadData (), send, send);
    }

    /// <summary>
    /// Closes the WebSocket connection asynchronously with the specified
    /// <see cref="ushort"/>, and releases all associated resources.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   This method doesn't wait for the close to be complete.
    ///   </para>
    ///   <para>
    ///   This method emits a <see cref="OnError"/> event if <paramref name="code"/>
    ///   isn't in the allowable range of the WebSocket close status code.
    ///   </para>
    /// </remarks>
    /// <param name="code">
    /// A <see cref="ushort"/> that represents the status code that indicates the
    /// reason for closure.
    /// </param>
    public void CloseAsync (ushort code)
    {
      CloseAsync (code, null);
    }

    /// <summary>
    /// Closes the WebSocket connection asynchronously with the specified
    /// <see cref="CloseStatusCode"/>, and releases all associated resources.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the close to be complete.
    /// </remarks>
    /// <param name="code">
    /// One of the <see cref="CloseStatusCode"/> enum values, represents the
    /// status code that indicates the reason for closure.
    /// </param>
    public void CloseAsync (CloseStatusCode code)
    {
      CloseAsync (code, null);
    }

    /// <summary>
    /// Closes the WebSocket connection asynchronously with the specified
    /// <see cref="ushort"/> and <see cref="string"/>, and releases all
    /// associated resources.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   This method doesn't wait for the close to be complete.
    ///   </para>
    ///   <para>
    ///   This method emits a <see cref="OnError"/> event if <paramref name="code"/>
    ///   isn't in the allowable range of the WebSocket close status code or the
    ///   size of <paramref name="reason"/> is greater than 123 bytes.
    ///   </para>
    /// </remarks>
    /// <param name="code">
    /// A <see cref="ushort"/> that represents the status code that indicates the
    /// reason for closure.
    /// </param>
    /// <param name="reason">
    /// A <see cref="string"/> that represents the reason for closure.
    /// </param>
    public void CloseAsync (ushort code, string reason)
    {
      byte [] data = null;
      var msg = checkIfCanClose (
        () => code.CheckIfValidCloseStatusCode () ??
              (data = code.Append (reason)).CheckIfValidControlData ("reason"));

      if (msg != null) {
        _logger.Error (
          String.Format ("{0}\ncode: {1} reason: {2}", msg, code, reason));

        error (msg);
        return;
      }

      var send = _readyState == WebSocketState.OPEN && !code.IsReserved ();
      closeAsync (new PayloadData (data), send, send);
    }

    /// <summary>
    /// Closes the WebSocket connection asynchronously with the specified
    /// <see cref="CloseStatusCode"/> and <see cref="string"/>, and releases all
    /// associated resources.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   This method doesn't wait for the close to be complete.
    ///   </para>
    ///   <para>
    ///   This method emits a <see cref="OnError"/> event if the size of
    ///   <paramref name="reason"/> is greater than 123 bytes.
    ///   </para>
    /// </remarks>
    /// <param name="code">
    /// One of the <see cref="CloseStatusCode"/> enum values, represents the
    /// status code that indicates the reason for closure.
    /// </param>
    /// <param name="reason">
    /// A <see cref="string"/> that represents the reason for closure.
    /// </param>
    public void CloseAsync (CloseStatusCode code, string reason)
    {
      byte [] data = null;
      var msg = checkIfCanClose (
        () => (data = ((ushort) code).Append (reason))
              .CheckIfValidControlData ("reason"));

      if (msg != null) {
        _logger.Error (
          String.Format ("{0}\ncode: {1} reason: {2}", msg, code, reason));

        error (msg);
        return;
      }

      var send = _readyState == WebSocketState.OPEN && !code.IsReserved ();
      closeAsync (new PayloadData (data), send, send);
    }

    /// <summary>
    /// Establishes a WebSocket connection.
    /// </summary>
    public void Connect ()
    {
      var msg = checkIfCanConnect ();
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      if (connect ())
        open ();
    }

    /// <summary>
    /// Establishes a WebSocket connection asynchronously.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the connect to be complete.
    /// </remarks>
    public void ConnectAsync ()
    {
      var msg = checkIfCanConnect ();
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      Func<bool> connector = connect;
      connector.BeginInvoke (
        ar => {
          if (connector.EndInvoke (ar))
            open ();
        },
        null);
    }

    /// <summary>
    /// Closes the WebSocket connection, and releases all associated resources.
    /// </summary>
    /// <remarks>
    /// This method closes the WebSocket connection with the
    /// <see cref="CloseStatusCode.AWAY"/>.
    /// </remarks>
    public void Dispose ()
    {
      Close (CloseStatusCode.AWAY, null);
    }

    /// <summary>
    /// Sends a binary <paramref name="data"/> using the WebSocket connection.
    /// </summary>
    /// <param name="data">
    /// An array of <see cref="byte"/> that represents the binary data to send.
    /// </param>
    public void Send (byte [] data)
    {
      var msg = checkIfCanSend (() => data.CheckIfValidSendData ());
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      var len = data.LongLength;
      if (len <= FragmentLength)
        send (
          Opcode.BINARY,
          len > 0 && _compression == CompressionMethod.NONE
          ? data.Copy (len)
          : data);
      else
        send (Opcode.BINARY, new MemoryStream (data));
    }

    /// <summary>
    /// Sends the specified <paramref name="file"/> as a binary data
    /// using the WebSocket connection.
    /// </summary>
    /// <param name="file">
    /// A <see cref="FileInfo"/> that represents the file to send.
    /// </param>
    public void Send (FileInfo file)
    {
      var msg = checkIfCanSend (() => file.CheckIfValidSendData ());
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      send (Opcode.BINARY, file.OpenRead ());
    }

    /// <summary>
    /// Sends a text <paramref name="data"/> using the WebSocket connection.
    /// </summary>
    /// <param name="data">
    /// A <see cref="string"/> that represents the text data to send.
    /// </param>
    public void Send (string data)
    {
      var msg = checkIfCanSend (() => data.CheckIfValidSendData ());
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      var rawData = Encoding.UTF8.GetBytes (data);
      if (rawData.LongLength <= FragmentLength)
        send (Opcode.TEXT, rawData);
      else
        send (Opcode.TEXT, new MemoryStream (rawData));
    }

    /// <summary>
    /// Sends a binary <paramref name="data"/> asynchronously
    /// using the WebSocket connection.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the send to be complete.
    /// </remarks>
    /// <param name="data">
    /// An array of <see cref="byte"/> that represents the binary data to send.
    /// </param>
    /// <param name="completed">
    /// An Action&lt;bool&gt; delegate that references the method(s) called when
    /// the send is complete. A <see cref="bool"/> passed to this delegate is
    /// <c>true</c> if the send is complete successfully; otherwise, <c>false</c>.
    /// </param>
    public void SendAsync (byte [] data, Action<bool> completed)
    {
      var msg = checkIfCanSend (() => data.CheckIfValidSendData ());
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      var len = data.LongLength;
      if (len <= FragmentLength)
        sendAsync (
          Opcode.BINARY,
          len > 0 && _compression == CompressionMethod.NONE
          ? data.Copy (len)
          : data,
          completed);
      else
        sendAsync (Opcode.BINARY, new MemoryStream (data), completed);
    }

    /// <summary>
    /// Sends the specified <paramref name="file"/> as a binary data
    /// asynchronously using the WebSocket connection.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the send to be complete.
    /// </remarks>
    /// <param name="file">
    /// A <see cref="FileInfo"/> that represents the file to send.
    /// </param>
    /// <param name="completed">
    /// An Action&lt;bool&gt; delegate that references the method(s) called when
    /// the send is complete. A <see cref="bool"/> passed to this delegate is
    /// <c>true</c> if the send is complete successfully; otherwise, <c>false</c>.
    /// </param>
    public void SendAsync (FileInfo file, Action<bool> completed)
    {
      var msg = checkIfCanSend (() => file.CheckIfValidSendData ());
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      sendAsync (Opcode.BINARY, file.OpenRead (), completed);
    }

    /// <summary>
    /// Sends a text <paramref name="data"/> asynchronously
    /// using the WebSocket connection.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the send to be complete.
    /// </remarks>
    /// <param name="data">
    /// A <see cref="string"/> that represents the text data to send.
    /// </param>
    /// <param name="completed">
    /// An Action&lt;bool&gt; delegate that references the method(s) called when
    /// the send is complete. A <see cref="bool"/> passed to this delegate is
    /// <c>true</c> if the send is complete successfully; otherwise, <c>false</c>.
    /// </param>
    public void SendAsync (string data, Action<bool> completed)
    {
      var msg = checkIfCanSend (() => data.CheckIfValidSendData ());
      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      var rawData = Encoding.UTF8.GetBytes (data);
      if (rawData.LongLength <= FragmentLength)
        sendAsync (Opcode.TEXT, rawData, completed);
      else
        sendAsync (Opcode.TEXT, new MemoryStream (rawData), completed);
    }

    /// <summary>
    /// Sends a binary data from the specified <see cref="Stream"/>
    /// asynchronously using the WebSocket connection.
    /// </summary>
    /// <remarks>
    /// This method doesn't wait for the send to be complete.
    /// </remarks>
    /// <param name="stream">
    /// A <see cref="Stream"/> from which contains the binary data to send.
    /// </param>
    /// <param name="length">
    /// An <see cref="int"/> that represents the number of bytes to send.
    /// </param>
    /// <param name="completed">
    /// An Action&lt;bool&gt; delegate that references the method(s) called when
    /// the send is complete. A <see cref="bool"/> passed to this delegate is
    /// <c>true</c> if the send is complete successfully; otherwise, <c>false</c>.
    /// </param>
    public void SendAsync (Stream stream, int length, Action<bool> completed)
    {
      var msg = checkIfCanSend (
        () => stream.CheckIfCanRead () ??
              (length < 1 ? "'length' must be greater than 0." : null));

      if (msg != null) {
        _logger.Error (msg);
        error (msg);

        return;
      }

      stream.ReadBytesAsync (
        length,
        data => {
          var len = data.Length;
          if (len == 0) {
            msg = "A data cannot be read from 'stream'.";
            _logger.Error (msg);
            error (msg);

            return;
          }

          if (len < length)
            _logger.Warn (
              String.Format (
                "A data with 'length' cannot be read from 'stream'.\nexpected: {0} actual: {1}",
                length,
                len));

          var sent = len <= FragmentLength
                   ? send (Opcode.BINARY, data)
                   : send (Opcode.BINARY, new MemoryStream (data));

          if (completed != null)
            completed (sent);
        },
        ex => {
          _logger.Fatal (ex.ToString ());
          error ("An exception has occurred while sending a data.", ex);
        });
    }

    /// <summary>
    /// Sets an HTTP <paramref name="cookie"/> to send with the WebSocket
    /// connection request to the server.
    /// </summary>
    /// <param name="cookie">
    /// A <see cref="Cookie"/> that represents the HTTP Cookie to send.
    /// </param>
    public void SetCookie (Cookie cookie)
    {
      lock (_forConn) {
        var msg = checkIfAvailable ("SetCookie", false, false) ??
                  (cookie == null ? "'cookie' must not be null." : null);

        if (msg != null) {
          _logger.Error (msg);
          error (msg);

          return;
        }

        lock (_cookies.SyncRoot) {
          _cookies.SetOrRemove (cookie);
        }
      }
    }

    /// <summary>
    /// custom http request/response headers.
    /// </summary>
    public NameValueCollection CustomHeaders {
      get {
        return _customHeaders;
      }
      protected set {
        _customHeaders = value;
      }
    }

    #endregion
  }
}
