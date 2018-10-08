


import { Injectable } from '@angular/core';
import { HubConnection } from '@aspnet/signalr';
import { environment } from '../../environments/environment';
import * as signalR from '@aspnet/signalr';
import { BehaviorSubject } from 'rxjs';

import { ConstantData, DeliveryUpdate, EventLocation, GoalStatusMessage, SentimentInfo } from '../models';

@Injectable()
export class SignalrClientService {
  private _hubConnection: HubConnection | undefined;
  private currentEvent = ConstantData.EventCityeName;
  private isConnected = false;

  public eventLocationInfoArr: EventLocation[] = [];
  public notesArr: string[] = [];
  public updatedLastNote = '';
  public sentimentInfo: SentimentInfo[] = [];
  public deliveryUpdate: DeliveryUpdate[] = [];
  public goalStatusMsg: GoalStatusMessage = { locationId: '', goalStatus: 0} ;
  public observableRouteUpdate: BehaviorSubject<DeliveryUpdate[]>;
  public observableGoalStatusMsg: BehaviorSubject<GoalStatusMessage>;
  messages: string[] = [];

  constructor() {
    console.log('Inititaing SignalR Service .... ');
    this.observableRouteUpdate = new BehaviorSubject<DeliveryUpdate[]>(this.deliveryUpdate);
    this.observableGoalStatusMsg = new BehaviorSubject<GoalStatusMessage>(this.goalStatusMsg);
   }

  connectSignalR() {
    if (!this.isConnected) {
    // SignalREndPoint :: https://lunchbagwebportal.eastus.cloudapp.azure.com/api/signalr
    this._hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.SignalREndPoint)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this._hubConnection.start().catch(err => console.error(err.toString())).then(this.requestUpdate);
    this.isConnected = true;

    this._hubConnection.on('ReceiveGoalUpdate', (message) => {
      this.updateEventLocationInfoArr(message);
    });

    this._hubConnection.on('ReceiveSentimentUpdate', (message) => {
      this.sentimentInfo = message;
      console.log('Invoking Sentiment:', message);
    });

    this._hubConnection.on('ReceiveNote', (message) => {
      console.log('Invoking Note : ', message);
      this.updatedLastNote = message.note;
      this.notesArr.push(message.note);
      });

    this._hubConnection.on('ReceiveDeliveryUpdate', (message) => {
          this.observableRouteUpdate.next(message);
      });

    this._hubConnection.on('Send', (data: any) => {
      const received = `Received: ${data}`;
      this.messages.push(received);
    });
  }
  }

  updateEventLocationInfoArr(argMsg: GoalStatusMessage) {
      this.eventLocationInfoArr.forEach( eventLoc => {
        if (eventLoc.id === argMsg.locationId) {
          eventLoc.goalStatus = argMsg.goalStatus;
        }
      });
      this.observableGoalStatusMsg.next(argMsg);
  }

  requestUpdate = () => {
    // TODO make view dynamic based on Url
    this._hubConnection
    .invoke('InitConnection', this.currentEvent, 'view1')
    .catch(err => console.error(err.toString())).then(this.readyCallBack);
  }

  readyCallBack = () => {
    console.log('SignalrR client is Ready to receive Data... ');
  }

  public sendMessage(argMessage: string): void {
    if (this._hubConnection) {
      this._hubConnection.invoke('Send', argMessage);
    }
    this.messages.push(argMessage);
  }
}
