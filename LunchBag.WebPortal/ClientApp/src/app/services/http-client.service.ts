import { Http } from '@angular/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class HttpClientService {

  constructor(private _http: Http) {}

  getEventDetails = (argCityName, argViewId = 'currentView') => {
    // EventsWSEndPoint="https://lunchbagwebportal.eastus.cloudapp.azure.com/api/Events/"
    // todo current view must be retrieved dynamically from url
    // let viewId = 'currentView';
    let finalUrl = environment.EventsWSEndPoint + argCityName + '/' + argViewId;
    return this._http.get(finalUrl);
  }

  getTransportDetails = (argEventCityName) => {
    // TransportWSEndPoint = https://lunchbagwebportal.eastus.cloudapp.azure.com/api/Transport/LasVegas/deliveries
    let finalUrl = environment.TransportWSEndPoint + argEventCityName + '/deliveries';
    return this._http.get(finalUrl);
  }

  getRoutesPoints = (argEventId , argRouteId) => {
    // RoutesPointWSEndPoint = https://lunchbagwebportal.eastus.cloudapp.azure.com/api/Transport/LasVegas/routes/c36cf5e5-74c9-4bb6-bb9c-95f1238c8b9c
    // RoutesPointWSEndPoint = https://lunchbagwebportal.eastus.cloudapp.azure.com/api/Transport/{eventId}/routes/{routeId}

    let routeWSEndPoint = environment.TransportWSEndPoint + argEventId + '/routes/' + argRouteId;
    return this._http.get(routeWSEndPoint);
  }
}
