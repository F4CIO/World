import { Component } from "@angular/core";
import  build  from "../build";
import { environment } from "../environments/environment";
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  template: `<router-outlet></router-outlet>`
})
export class AppComponent {
  constructor(private titleService: Title) {
      console.log(
          `\n%cBuild Info:\n\n` +
              `%c ❯ Environment: %c${environment.production ? "production 🏭" : "development 🚧"}\n` +
              `%c ❯ Build Version: ${build.version}\n` +
              ` ❯ Build Timestamp: ${build.timestamp}\n` +
              ` ❯ Build Message: %c${build.message || "<no message>"}\n`,
          "font-size: 14px; color: #7c7c7b;",
          "font-size: 12px; color: #7c7c7b",
          environment.production ? "font-size: 12px; color: #95c230;" : "font-size: 12px; color: #e26565;",
          "font-size: 12px; color: #7c7c7b",
          "font-size: 12px; color: #bdc6cf",
      );
      this.titleService.setTitle(environment.INSTANCE_NAME);
  }
}
