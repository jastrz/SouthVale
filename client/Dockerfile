FROM node:22-alpine AS build
ARG VITE_API_URL=
ARG VITE_BASE_URL=
ARG VITE_CONTACT_EMAIL=
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN VITE_API_URL=${VITE_API_URL} VITE_BASE_URL=${VITE_BASE_URL} VITE_CONTACT_EMAIL=${VITE_CONTACT_EMAIL} npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
