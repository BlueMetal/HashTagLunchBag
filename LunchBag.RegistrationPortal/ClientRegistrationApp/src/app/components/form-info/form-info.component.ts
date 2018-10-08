
import { Donate, Donation, Donor, PaymentType } from './../../models';
import { HttpClientService } from '../../services/http-client.service';
import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";

@Component({
  selector: "app-form-info",
  templateUrl: "./form-info.component.html",
  styleUrls: ["./form-info.component.scss"]
})
export class FormInfoComponent implements OnInit {
  @Input() donate: Donate;
  @Output() eventFromForm = new EventEmitter<object>();


  input_number = null;
  amount = "10";
  people = "5";
  bags = Array(5).fill("");

  constructor(private _httpClient: HttpClientService) { }
  ngOnInit() { }

  onFocus(event: any) {
    event.target.value = 1;
    this.amount = "0";
    this.donate.amount = 0;
  }

  onKey(event: any) {
    const num = Number(event.target.value);
    const val = String(num);
    this.amount = val;
    this._algorithm(num);
    this.donate.amount = num;
  }

  doAmount(val: string) {
    const num = Number(val);
    this.input_number = null;
    this.amount = val;
    this._algorithm(num);
    this.donate.amount = Number(val);
  }

  _algorithm(num) {
    let bgs = 0;
    let ppl = "0";
    if (num > 0) {
      bgs = 2;
      ppl = "about 2";
    }
    if (num >= 10) {
      bgs = 5;
      ppl = "about 5";
    }
    if (num >= 15) {
      bgs = 8;
      ppl = "about 8";
    }
    if (num >= 20) {
      bgs = 10;
      ppl = "about 10";
    }
    if (num >= 25) {
      bgs = 20;
      ppl = "around 15-20";
    }
    if (num >= 30) {
      bgs = 25;
      ppl = "more than 20";
    }
    if (num >= 40) {
      bgs = 25;
      ppl = "a lot of";
    }
    this.bags = Array(bgs).fill("");
    this.people = ppl;
  }



  onSubmit() {
    let donation: Donation = {
      type: 'PayPal',
      reference: this.donate.donor.email,
      amount: this.donate.amount
    };

    if (!this.donate.donor.userExist) {

      this._httpClient.registerDonor(this.donate.donor).subscribe(res => {
        console.log('Donor: ', this.donate.donor.firstName, '- has registered successfully. Response: ');
        if (this.donate.type === PaymentType.CreditCard) {
          this._httpClient.makeDonation(donation).subscribe(donationResonse => {
            console.log('Successfully Donated for new user through PayPal  ', this.donate.donor.firstName);
          }, donationError => {
            console.warn('Error while Donation for new user', this.donate.donor.firstName);
          }
          );
        } else {
          console.log(this.donate.donor.firstName, ' - Should Provide the Cash in the counter');
        }
      });
    } else {
      if (this.donate.type === PaymentType.CreditCard) {
        this._httpClient.makeDonation(donation).subscribe(
          res => {
            console.log('Successfully Donated for existing donor via Paypal:   ', this.donate.donor.firstName);
          },
          err => {
            console.warn('Error while Donation for existing donor via Paypal: ', this.donate.donor.firstName);
          }
        );
      } else {
        console.log(this.donate.donor.firstName, ' - Should Provide the Cash in the counter');
      }
    }


    this.eventFromForm.emit({});
  }
}
