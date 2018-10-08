
import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { HttpClient, HttpHeaders } from "@angular/common/http";

import { Donation, Donor } from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class HttpClientService {

  // Swagger :: https://donate.ready.gives/api/swagger/index.html

  private EventId = 'LasVegas';
  private LocationId = 'LocLasVegas';

  constructor(private _httpClient: HttpClient) { }

  getRegisteredDonorInfo(userID: string) {
    let apiURL = environment.RegistrationsWSEndPoint + userID;

    let headers = new HttpHeaders();
    //headers = headers.append("Authorization", "Basic " + btoa("admin:lunchbag1234")); // TODO: Romove Authentication
    headers = headers.append("Content-Type", "application/x-www-form-urlencoded");

    return this._httpClient.get(apiURL, {headers: headers});
  }


  registerDonor(donor: Donor): Observable<Object> {
    // RegistrationsWSEndPoint : 'https://donate.ready.gives/api/Registrations/{eventId}/{locationId}',
    // let regDonorPostUrl = environment.RegistrationsWSEndPoint + this.EventId + '/' + this.LocationId;
    let regDonorPostUrl = environment.RegistrationsWSEndPoint;

    let payload: any = donor;
    payload.eventId = this.EventId;
    payload.locationId = this.LocationId;

    return this._httpClient.post(regDonorPostUrl, payload);
  }

  makeDonation(donation: Donation): Observable<Object> {
    // DonationWSEndPoint: https://donate.ready.gives/api/Registrations/{eventId}/{locationId}/donations/{email}
    // let donationPostUrl = environment.RegistrationsWSEndPoint + this.EventId + '/' + this.LocationId + '/donations/'+ donation.reference;
    let donationPostUrl = environment.RegistrationsWSEndPoint + '/donations';

    let payload: any = donation;
    payload.eventId = this.EventId;
    payload.locationId = this.LocationId;
    payload.email = donation.reference;

    return this._httpClient.post(donationPostUrl, payload);
  }

}
