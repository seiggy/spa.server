# react.server

react.server is a .NET 8 ASP.NET Core generic hosting app designed to facilitate the hosting of React Single Page Applications (SPAs) behind the Kestrel web server within a Docker container. This project provides a seamless way to serve your React applications with ease and includes various enhancements like configuration dumping and static file compression.

### Why?
Yeah, you can host react using nginx or any other web server, and all that fun jazz. However, there's some constraints that frustrated me about it. No way to load environment specific variables without building shell scripts to copy them to a file on start-up. Having to use nginx config files, when I'm a .NET/C# dev - just outside my comfort zone and I've only got so much time in a day. Kestrel is light and fast, and easy to configure with my primary skills, so it made sense for me to use this. Also way easier for me to add in a variety of systems, and I'll probably even add in MEF to support drop-in extensions. So yeah, mostly just a personal project for lowering my support spread for the times I needed to host a SPA in a container.

## Features

- **Easy Hosting**: Simplifies the process of hosting React SPAs using ASP.NET Core.
- **Configuration API**: Provides an API endpoint at `/api/configuration` that outputs environment variables or app settings from the `{"react": {}}` variable group in the `IConfiguration` store to a JavaScript file that attaches these variables to the `Window` object.
- **Static File Compression**: Enables static file compression for improved performance.
- **Extensibility**: Allows integration with tools like Application Insights for enhanced server visibility.

## Planned
- Content Security Policy configuration support - need this for Teams app hosting.
- X-Frame-Options configuration support - also, necessary for Teams tab hosting.
- Configuration extensions and documentation for Kestrel options.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

### Installation

1. Create a Dockerfile that uses this as a build container:

```Dockerfile
FROM node:18-alpine as builder
WORKDIR /code/
ADD package-lock.json .
ADD package.json .
RUN npm ci
ADD . .
RUN npm run build

FROM seiggy/react.server:latest
EXPOSE 80
COPY --from=builder /code/build /app/wwwroot
```

### Configuration

To configure the application, you can set environment variables or app settings in the `{"react": {}}` group within the `IConfiguration` store. These settings will be accessible via the `/api/configuration` endpoint. If you want to use Environment variables, prefix your variable with `react__` to have .NET automatically pick it up and add it to the config. Example: `react__apiUrl`.

### Example Configuration

```json
{
  "react": {
    "apiUrl": "https://api.yourdomain.com",
    "environment": "production"
  }
}
```

### Accessing Configuration

You can access the configuration by adding a script tag to your `index.html` page in the header.

```html
<script src="/api/configuration" type="application/javascript"></script>
```

Then modify your `config.ts` react component like so:

```typescript
declare global {
  interface Window {
    env: any;
  }
}

const config = {
  apiEndpoint: process.env.REACT_APP_API_SERVER_URL || window.env.apiUrl,
};

export default config;
```

## Static File Compression

Static file compression is enabled by default to improve performance. This helps in reducing the size of the files sent to the client, resulting in faster load times.

## Extensibility

You can integrate tools like Application Insights or other monitoring tools to gain better visibility into your server's performance. Add the necessary configurations and dependencies as required.

## Contributing

We welcome contributions! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/your-feature`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Feel free to reach out if you have any questions or need further assistance!

Happy coding!