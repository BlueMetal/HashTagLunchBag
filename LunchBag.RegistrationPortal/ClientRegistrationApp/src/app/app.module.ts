import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NgModule } from "@angular/core";
import { HttpClientModule } from '@angular/common/http';
import { HttpModule } from '@angular/http';
import { AppComponent } from "./app.component";
import { StartComponent } from "./pages/start/start.component";
import { FormStartComponent } from "./components/form-start/form-start.component";
import { RegisterComponent } from './pages/register/register.component';
import { FormInfoComponent } from './components/form-info/form-info.component';
import { CompleteComponent } from './pages/complete/complete.component';

@NgModule({
  declarations: [AppComponent, StartComponent, FormStartComponent, RegisterComponent, FormInfoComponent, CompleteComponent],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    HttpModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
