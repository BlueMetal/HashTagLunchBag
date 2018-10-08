
import { HttpClientService } from '../../services/http-client.service';

import { ConstantData, Delivery, Transport } from '../../models';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'all-cars-map',
  templateUrl: './all-cars-map.component.html',
  styleUrls: ['./all-cars-map.component.scss']
})
export class AllCarsMapComponent implements OnInit {
  private transportData: Transport = { count: 0, totalMiles: 0, totalLunches: 0, deliveries: [] };
  private deliveryArr: Delivery[];
  private deliveryCouter = -1;
  private dataFlag = false;
  private deliveryCarCount = 0;
  public displayAllCars = true;
  public mapDataArr: Delivery[];
  public deliveryCarStatusBox = {
    navigationButtonText: 'ALL CARS',
    icon: '../../../assets/icons/heart.svg',
    milesToDestValue: 0,
    milesToDestText: 'DELIVERY CARS',
    lunchMade: '0',
    milesTravled: 0
  };


  constructor(private _httpClient: HttpClientService,
    ) { }

  ngOnInit() {
    this._httpClient.getTransportDetails(ConstantData.EventCityeName)
      .subscribe(response => {
        this.transportData = response.json();
        // status:0 -> car not started | status: 1 -> Car is running status:2 -> reached to dest
        this.deliveryArr = this.transportData.deliveries.filter(item => item.status !== 0 );
        this.deliveryCarCount = this.deliveryArr.length;
        this.deliveryCarStatusBox.milesToDestValue = this.deliveryCarCount ;
        this.deliveryCarStatusBox.lunchMade = this.transportData.totalLunches.toLocaleString();
        this.deliveryCarStatusBox.milesTravled = this.getTotalMilesTraveled(this.transportData.deliveries);
        this.mapDataArr = this.transportData.deliveries;
        this.dataFlag = true;
      });
  }

  getTotalMilesTraveled(argDeliveryArr: Delivery[]): number {
    let retTotalTraveledMiles = 0;
    argDeliveryArr.forEach(delivery => {
      retTotalTraveledMiles += delivery.milesTraveled;
    });
    return Math.round(retTotalTraveledMiles);
  }


  handleLeftAngleClick() {
    if (this.deliveryCouter < 0) this.deliveryCouter = this.deliveryCarCount;
    this.deliveryCouter--;
    this.updateDeliveryCarStausBox();
  }

  handleRightAngleClick() {
    if (this.deliveryCouter >= this.deliveryCarCount) this.deliveryCouter = -1;
    this.deliveryCouter++;
    this.updateDeliveryCarStausBox();

  }

  updateDeliveryCarStausBox() {
    if (this.deliveryCouter < 0 || this.deliveryCouter >= this.deliveryCarCount ) {
      this.deliveryCarStatusBox.navigationButtonText = 'ALL CARS',
      this.deliveryCarStatusBox.icon = '../../../assets/icons/heart.svg';
      this.deliveryCarStatusBox.milesToDestValue = this.deliveryCarCount;
      this.mapDataArr = this.transportData.deliveries;
      this.deliveryCarStatusBox.milesToDestText = 'DELIVERY CARS';
      this.deliveryCarStatusBox.lunchMade = this.transportData.totalLunches.toLocaleString();
      this.deliveryCarStatusBox.milesTravled = this.getTotalMilesTraveled(this.transportData.deliveries);
      this.displayAllCars = true;
    } else {
      this.deliveryCarStatusBox.navigationButtonText =  this.deliveryArr[this.deliveryCouter].vehicleName;
      this.deliveryCarStatusBox.icon = '../../../assets/icons/flag.png';
      this.deliveryCarStatusBox.milesToDestValue = Math.round(this.deliveryArr[this.deliveryCouter].milesToDestination);
      this.deliveryCarStatusBox.milesToDestText = 'MILES TO DEST.';
      this.deliveryCarStatusBox.lunchMade = this.deliveryArr[this.deliveryCouter].lunchCount.toLocaleString();
      this.deliveryCarStatusBox.milesTravled = Math.round(this.deliveryArr[this.deliveryCouter].milesTraveled);
      this.mapDataArr = [this.deliveryArr[this.deliveryCouter]];
      this.displayAllCars = false;
      return this.deliveryArr[this.deliveryCouter].vehicleName;
    }
  }

  getMapData():  Delivery[] {
    let retData: Delivery[] = null;
    if (this.dataFlag) retData = this.mapDataArr;
    return retData;
  }


}
