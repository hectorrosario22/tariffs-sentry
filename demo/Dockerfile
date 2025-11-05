# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json pnpm-lock.yaml* ./

# Install dependencies
RUN npm install -g pnpm && pnpm install --frozen-lockfile

# Copy source code
COPY . .

# Build application
RUN pnpm build

# Runtime stage
FROM node:18-alpine

WORKDIR /app

# Install simple HTTP server
RUN npm install -g http-server

# Copy built app from build stage
COPY --from=build /app/dist ./dist

EXPOSE 80

# Serve the application
CMD ["http-server", "dist", "-p", "80", "-c-1"]
