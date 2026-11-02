# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build

# Production stage
FROM node:18-alpine

WORKDIR /app

# Установить serve для обслуживания production build
RUN npm install -g serve

COPY --from=builder /app/build ./build

EXPOSE 3000

ENV PORT=3000

CMD ["serve", "-s", "build", "-l", "3000"]
