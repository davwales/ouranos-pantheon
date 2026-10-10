FROM node:22.23-alpine

WORKDIR /app

COPY ./src/apps/interface/package*.json ./
RUN npm ci

COPY ./src/apps/interface .
RUN npm run build

EXPOSE 3000
CMD ["npm", "start"]