# Stage 1: Build Angular application
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration=production

# Stage 2: Serve using Nginx
FROM nginx:alpine
COPY --from=build /app/dist/portfolio/browser /usr/share/nginx/html
# Custom nginx configuration for SPA routing
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
