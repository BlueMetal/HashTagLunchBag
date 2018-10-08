import { LocationPoint } from './location-point';

export interface RoutePoints {
    id: string;
    routeId: string;
    count: number;
    points: LocationPoint[];
}
