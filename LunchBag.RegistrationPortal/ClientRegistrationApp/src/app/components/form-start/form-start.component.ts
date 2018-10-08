import { Donate, Donor } from './../../models';
import { HttpClientService } from '../../services/http-client.service';
import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";


@Component({
  selector: "app-form-start",
  templateUrl: "./form-start.component.html",
  styleUrls: ["./form-start.component.scss"]
})
export class FormStartComponent implements OnInit {
  @Input() donate: Donate;
  @Output() eventFromForm = new EventEmitter<object>();

  constructor(private _httpClient: HttpClientService) { }
  ngOnInit() { }

  onSubmit() {
    this._httpClient.getRegisteredDonorInfo(this.donate.donor.email).subscribe(
      (res: Donor) => {
        if (res.userExist) {
          this.donate.donor.firstName = res.firstName;
          this.donate.donor.lastName = res.lastName;
          this.donate.donor.userExist = true;
        }
      },
      err => {
        console.warn('Error while getting Registerd User: ', err);
      });
    this.eventFromForm.emit({});
  }
}
