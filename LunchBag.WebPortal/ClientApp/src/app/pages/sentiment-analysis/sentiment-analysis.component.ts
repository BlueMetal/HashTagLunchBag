import { Component, OnInit } from '@angular/core';
import { SignalrClientService } from '../../services/signalr-client.service';
import { HttpClientService } from '../../services/http-client.service';

@Component({
  selector: 'app-sentiment-analysis',
  templateUrl: './sentiment-analysis.component.html',
  styleUrls: ['./sentiment-analysis.component.scss']
})
export class SentimentAnalysisComponent implements OnInit {

  constructor(private _httpClient: HttpClientService,
    public _signalRClient: SignalrClientService) { }

  ngOnInit() {
    this._httpClient.getEventDetails('LasVegas').subscribe(response => {
      const data = response.json();
      this._signalRClient.updatedLastNote = data.lastNote;
      this._signalRClient.sentimentInfo = data.eventSentiments;
      console.log('Initial Event Info Result', this._signalRClient.sentimentInfo);
      this._signalRClient.connectSignalR();
    });

  }

  getSentimentPercentage(argSentiment: string): string {
    let retPercentage = '0%';
    if (this._signalRClient.sentimentInfo.length > 0) {
      this._signalRClient.sentimentInfo.forEach(element => {
        if (element.name.toLowerCase() === argSentiment) {
          retPercentage = Math.round(element.percentage) + '%';
        }
      });
    }
    return retPercentage;
  }

  getLastNote(): string {
    let lastNote = this._signalRClient.updatedLastNote;
    lastNote = lastNote.replace(/hope/ig, '<u>hope</u>');
    lastNote = lastNote.replace(/love/ig, '<u>love</u>');
    lastNote = lastNote.replace(/care/ig, '<u>care</u>');
    return lastNote;
  }

}
