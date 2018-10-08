
import { Component, OnInit } from "@angular/core";
import { Donate, Donor, NavSteps, PaymentType } from './models';

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"]
})
export class AppComponent implements OnInit {

  donorObj: Donor = {
    email: "",
    firstName: "",
    lastName: "",
    zipCode: "",
    userExist: false
  };

  donate: Donate = {
    donor: this.donorObj,
    type: PaymentType.CreditCard,
    amount: 0,
  };

  step = NavSteps.Start;

  constructor() { }

  ngOnInit() { }

  eventFromPage(passed) {
    this.step = passed.step;
    window.scrollTo(0, 0);
  }
}
