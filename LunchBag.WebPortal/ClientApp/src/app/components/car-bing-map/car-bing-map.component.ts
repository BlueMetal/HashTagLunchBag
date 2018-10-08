
/// <reference path="../../../../node_modules/bingmaps/types/MicrosoftMaps/CustomMapStyles.d.ts" />
/// <reference path="../../../../node_modules/bingmaps/types/MicrosoftMaps/Microsoft.Maps.All.d.ts" />
/// <reference path="../../../../node_modules/bingmaps/types/MicrosoftMaps/Microsoft.Maps.d.ts" />
/// <reference path="../../../../node_modules/bingmaps/types/MicrosoftMaps/ConfigurationDrivenMaps.d.ts" />

import { Component, OnInit, AfterViewInit, ViewChild, Input, OnChanges } from '@angular/core';
import { ConstantData, Delivery, DeliveryUpdate, LocationPoint, RoutePoints } from '../../models';
import { HttpClientService } from '../../services/http-client.service';
import { SignalrClientService } from '../../services/signalr-client.service';
// import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'car-bing-map',
  templateUrl: './car-bing-map.component.html',
  styleUrls: ['./car-bing-map.component.scss']
})
export class CarBingMapComponent implements OnInit, AfterViewInit, OnChanges {

  @Input() deliveryCarInfoArr: Delivery[];
  @ViewChild('carMap') carMap = null; // using ViewChild to reference the div instead of setting an id

  public pageTitle = 'Map';
  public carBingMap: Microsoft.Maps.Map;
  public mapLayer: Microsoft.Maps.Layer;
  private bingMapCredential = 'Aqnh8lI4ylnXZxBpJQmKi8BeWVTm6QZJJ2iFlhohsXY3Bkw3wItis51zgdefh2IW';
  private roundPushPinIcon = '<svg xmlns="http://www.w3.org/2000/svg" width="50" height="50"><circle cx="25" cy="25" r="10" stroke="white" stroke-width="4" fill="#CE4159" /></svg>';
  private pushPinIconPath = '../../../assets/icons/pushpin-heart.svg';
  private redColor = '#CE4159';
  private carRouteUpdate: DeliveryUpdate[] = [];
  private mapMonitorCounter = 0;

  private customMapVisualSetting = {
    'version': '1.0',
    elements: {
      area: { fillColor: '#b6e591' },
      water: { fillColor: '#01A4EF' },
      tollRoad: { fillColor: '#35C7DE', strokeColor: '#35C7DE' },
      arterialRoad: { fillColor: '#35C7DE', strokeColor: '#35C7DE' },
      road: { fillColor: '#35C7DE', strokeColor: '#35C7DE' },
      street: { fillColor: '#CFCFCF ', strokeColor: '#CFCFCF' },
      transit: { fillColor: '#000000' }
    },
    settings: {
      landColor: '#4A4A4A'
    }
  };

  constructor(private _httpClient: HttpClientService,
    public _signalRClient: SignalrClientService) {
      // this.observableRouteUpdate = new BehaviorSubject<DeliveryUpdate[]>(this._signalRClient.deliveryUpdate);
    }

  ngOnInit() {

  }

  ngOnChanges() {
    if (this.deliveryCarInfoArr && this.deliveryCarInfoArr.length > 0) {
      this.removeLines();
      this.mapLayer.clear();
      this.renderCarMap(this.carBingMap);
    }


  }


  ngAfterViewInit() {
    this.carBingMap = this.initiateCarMap();
    this.mapLayer = new Microsoft.Maps.Layer();
    this._signalRClient.connectSignalR();
    this.carRouteUpdate = this._signalRClient.deliveryUpdate;
    this.renderCarMap(this.carBingMap);

    this._signalRClient.observableRouteUpdate.subscribe(item => {
      this.removeLines();
      this.mapLayer.clear();
      this.renderCarMap(this.carBingMap);
    });
  }

  initiateCarMap(): Microsoft.Maps.Map {
    let navigationBarMode = Microsoft.Maps.NavigationBarMode;
    let allCarMap = new Microsoft.Maps.Map(this.carMap.nativeElement, {
      credentials: this.bingMapCredential,
      mapTypeId: Microsoft.Maps.MapTypeId.grayscale,
      navigationBarMode: navigationBarMode.minified,
      zoom: 10,
      customMapStyle: this.customMapVisualSetting
    });

    return allCarMap;
  }

  renderCarMap(argBingMaps: Microsoft.Maps.Map) {
    // after the view completes initializaion, create the map
    if (this.deliveryCarInfoArr && this.deliveryCarInfoArr.length > 0) {
      for (let cnt = 0; cnt < this.deliveryCarInfoArr.length; cnt++) {
        if (this.deliveryCarInfoArr[cnt].routeId) {
          this._httpClient.getRoutesPoints(ConstantData.EventCityeName, this.deliveryCarInfoArr[cnt].routeId)
            .subscribe(response => {
              {
                let routePointResponse: RoutePoints = response.json();
                let cordinatesArr = this.getCoOrdinatorsArr(routePointResponse.points);
                // set the map center =======================================
                argBingMaps.setView({
                  mapTypeId: Microsoft.Maps.MapTypeId.grayscale,
                  center: cordinatesArr[cordinatesArr.length - 1]
                });

                // set MapLine ============================================================
                let mapLine = new Microsoft.Maps.Polyline(cordinatesArr, {
                  strokeColor: this.redColor,
                  strokeThickness: 4
                });

                argBingMaps.entities.push(mapLine);

                let startPushPin = new Microsoft.Maps.Pushpin(cordinatesArr[0], {
                  icon: this.roundPushPinIcon,
                  anchor: new Microsoft.Maps.Point(25, 25)
                });
                let destPushPin = new Microsoft.Maps.Pushpin(cordinatesArr[cordinatesArr.length - 1], {
                  icon: this.pushPinIconPath,
                  anchor: new Microsoft.Maps.Point(20, 55)
                });

                this.mapLayer.add(startPushPin);
                this.mapLayer.add(destPushPin);
                argBingMaps.layers.insert(this.mapLayer);
              }
            });
        }
      } // -- End of IF
    }
  }

  // ============ Convert Location Points to Microsoft.Maps.Location[] =============================
  getCoOrdinatorsArr(argPointsArr: LocationPoint[]): Microsoft.Maps.Location[] {
    let retCordinatorArr: Microsoft.Maps.Location[] = [];
    argPointsArr.forEach(points => {
      retCordinatorArr.push(new Microsoft.Maps.Location(points.latitude, points.longitude));
    });

    return retCordinatorArr;
  }

  getCarDestination(): Microsoft.Maps.Location {
    if (this.deliveryCarInfoArr && this.deliveryCarInfoArr.length > 0) {
      return this.getMicrosoftMapsLocation(this.deliveryCarInfoArr[0].destinationLocation);
    } else return new Microsoft.Maps.Location(36.137132, -115.27826);
  }

  removeLines = () => {
    if (this.carBingMap.entities && this.carBingMap.entities.getLength() > 0) {
      for (let i = this.carBingMap.entities.getLength() - 1; i >= 0; i--) {
        let polyline = this.carBingMap.entities.get(i);
        if (polyline instanceof Microsoft.Maps.Polyline) {
          this.carBingMap.entities.removeAt(i);
        }
      }
    }
  }

  getMicrosoftMapsLocation(argLocation: LocationPoint): Microsoft.Maps.Location {
    return new Microsoft.Maps.Location(argLocation.latitude, argLocation.longitude);
  }
}
