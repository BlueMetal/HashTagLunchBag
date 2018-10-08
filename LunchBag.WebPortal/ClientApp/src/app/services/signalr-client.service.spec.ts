import { TestBed, inject } from '@angular/core/testing';

import { SignalrClientService } from './signalr-client.service';

describe('SignalrClientService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SignalrClientService]
    });
  });

  it('should be created', inject([SignalrClientService], (service: SignalrClientService) => {
    expect(service).toBeTruthy();
  }));
});
