# Yalniz gelistirici makinesindeki sentetik/test secret'lari icindir.
# Production listener daima TLS, auto-unseal ve audit ile ayri kurulmalidir.
storage "file" {
  path = "/vault/file"
}

listener "tcp" {
  address     = "0.0.0.0:8200"
  tls_disable = 1
}

api_addr      = "http://127.0.0.1:8200"
disable_mlock = true
ui            = true
