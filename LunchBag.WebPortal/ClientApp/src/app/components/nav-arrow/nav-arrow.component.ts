import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'nav-arrow',
  templateUrl: './nav-arrow.component.html',
  styleUrls: ['./nav-arrow.component.scss']
})
export class NavArrowComponent implements OnInit {

  @Input() upArrowRoutePath = '';
  @Input() downArrowRoutePath = '';

  constructor(private router: Router) { }

  ngOnInit() {
  }

  handleUpClick() {
    this.router.navigate([this.upArrowRoutePath]);
  }

  handleDownClick() {
    this.router.navigate([this.downArrowRoutePath]);
  }
}
