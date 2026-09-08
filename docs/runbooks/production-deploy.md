# Security Awareness 2 — Production deployment checklist

## Required IIS environment variables

Set these in IIS → Application Pool → Advanced Settings, OR via `web.config` `<environmentVariables>`:

| Variable | Example | Notes |
|----------|---------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | |
| `ConnectionStrings__Default` | `Server=prod-sql;Database=SecurityAwareness2Db;User Id=sa;Password=***;TrustServerCertificate=True;Encrypt=True;` | **Never commit real password** |
| `Tracking__BaseUrl` | `https://awareness.egit.com.tw` | Public HTTPS URL employees will hit |
| `AdminBootstrap__Password` | (set first deploy, then change) | Initial seed password |
| `Email__Graph__TenantId` | `00000000-...` | From IT |
| `Email__Graph__ClientId` | `aaaa...` | From IT |
| `Email__Graph__ClientSecret` | (very long random string) | From IT, expires 24 months |

## Production deployment steps

1. **Build publish**
   ```cmd
   dotnet publish src/SecurityAwareness.Platform -c Release -o artifacts/publish
   ```

2. **Copy to IIS server**
   - Copy entire `artifacts/publish/` to `C:\inetpub\sites\SecurityAwareness2\`

3. **Set environment variables** in IIS Application Pool

4. **Set DataProtection keys folder**
   - For multi-instance: use a UNC share (`\\fileserver\sa2-keys\`)
   - Set `DataProtection__KeyDirectory` env var pointing to that path (or update Program.cs)

5. **First-run admin password change**
   - Default seed is `admin / Admin@123`
   - Log in, immediately change the password via SQL or future UI:
   ```sql
   UPDATE SystemAccounts SET PasswordHash = '<BCrypt hash>' WHERE Username = 'admin';
   ```

6. **HTTPS certificate**
   - Bind SSL cert in IIS
   - Cookie `SecurePolicy` is already set to `Always` in Production

7. **Firewall**
   - Open only 80/443 inbound
   - Restrict /Admin/* by IP allowlist (use IIS IP Restrictions)

8. **Backup**
   - SQL: nightly full + hourly differential via SQL Agent / maintenance plan
   - `D:\SA2\artifacts\eml\` if using Fake mode (purge after 30 days)

## Health check
- `GET /health/live` — server is up
- `GET /health/ready` — DB + Email queue is reachable

## Logging
- Logs go to `C:\inetpub\sites\SecurityAwareness2\Logs\` (Serilog rolling daily)
