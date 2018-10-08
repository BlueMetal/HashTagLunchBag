import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { Routes, RouterModule } from '@angular/router';
import { SignalrClientService } from './services/signalr-client.service';
import { AngularFontAwesomeModule } from 'angular-font-awesome';
import { HttpClientService } from './services/http-client.service';
import { AppComponent } from './app.component';
import { HomeComponent } from './pages/home/home.component';
import { AllCitiesStatusComponent } from './pages/all-cities-status/all-cities-status.component';
import { ProgressbarHeartComponent } from './components/progressbar-heart/progressbar-heart.component';
import { ProgressbarCityComponent } from './components/progressbar-city/progressbar-city.component';
import { HeartIconComponent } from './components/progressbar-city/heart-icon.component';
import { AllCarsMapComponent } from './pages/all-cars-map/all-cars-map.component';
import { SentimentAnalysisComponent } from './pages/sentiment-analysis/sentiment-analysis.component';
import { GiveSvgComponent } from './components/give-svg/give-svg.component';
import { StatusSquareComponent } from './components/status-square/status-square.component';
import { TrackGivingComponent } from './components/track-giving/track-giving.component';
import { CarBingMapComponent } from './components/car-bing-map/car-bing-map.component';
import { NavArrowComponent } from './components/nav-arrow/nav-arrow.component';


const appRoutes: Routes = [
  { path: '', redirectTo: '/organize', pathMatch: 'full' },
  {path: 'organize', component: HomeComponent},
  {path: 'organize/:viewId', component: HomeComponent},
  {path: 'prepare', component: AllCitiesStatusComponent},
  {path: 'progressbar-city', component: ProgressbarCityComponent},
  {path: 'give', component: AllCarsMapComponent},
  { path: 'sentiment', component: SentimentAnalysisComponent },
  { path: '**', redirectTo: '/organize', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    AllCitiesStatusComponent,
    ProgressbarHeartComponent,
    ProgressbarCityComponent,
    HeartIconComponent,
    AllCarsMapComponent,
    SentimentAnalysisComponent,
    GiveSvgComponent,
    StatusSquareComponent,
    TrackGivingComponent,
    CarBingMapComponent,
    NavArrowComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AngularFontAwesomeModule,
    FormsModule,
    HttpModule,
    RouterModule.forRoot(appRoutes)
  ],
  providers: [
    SignalrClientService,
    HttpClientService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
