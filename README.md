# coolApi

ASP.NET Core (.NET 10) + PostgreSQL. Needs [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker](https://docs.docker.com/get-docker/).

## Run (Windows & Linux)

```bash
git clone https://github.com/heppycat/coolapi.git coolApi && cd coolApi
docker compose up -d
dotnet run --project src/Api --launch-profile http
```

Open **http://localhost:5054** for the UI.

- Login: `admin@example.com` / `Admin123!` (or register a new user)
- API docs: http://localhost:5054/scalar

On first start (Development) the DB is migrated and seeded with sample products and customers.

Stop: `docker compose down` (add `-v` to wipe the data).
