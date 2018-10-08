import { LocationPoint } from './location-point';
export interface Geocode {
    location: LocationPoint;
    formattedAddress: string;
    confidence: string;
}