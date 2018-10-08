import { Geocode } from './geocode';
import { LocationPoint } from './location-point';

export interface Delivery {
    id: string;
    deliveryId: string;
    vehicleId: string;
    vehicleName: string;
    driverId: string;
    driverName: string;
    destination: string;
    destinationLocation: LocationPoint;
    destinationGeocode: Geocode;
    lunchCount: number;
    eventId: string;
    locationId: string;
    status: number;
    routeId: string;
    milesToDestination: number;
    milesTraveled: number;
    date: Date;
    eTag: string;
}
