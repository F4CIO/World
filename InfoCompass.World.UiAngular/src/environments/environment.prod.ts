import { NgxLoggerLevel } from 'ngx-logger';

export const environment = {
  production: true,
  logLevel: NgxLoggerLevel.OFF,
  serverLogLevel: NgxLoggerLevel.ERROR,
  API_BASE_URL: "https://www.world.com/backend",
  INSTANCE_NAME: "World"
};
