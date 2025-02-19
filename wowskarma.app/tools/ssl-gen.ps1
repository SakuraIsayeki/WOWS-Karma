# Install mkcert using winget
winget install -e --id "FiloSottile.mkcert"
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")


# Create a local CA (Certificate Authority)
mkcert -install

#mkdir
New-Item -ItemType Directory -Force -Path ./.ssl

# Generate an SSL certificate for localhost, save in ./ssl/ folder
mkcert -ecdsa -key-file ./.ssl/localhost.key -cert-file ./.ssl/localhost.crt localhost
