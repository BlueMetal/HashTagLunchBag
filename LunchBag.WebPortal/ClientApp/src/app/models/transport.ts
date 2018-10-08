import { Delivery } from './delivery';

export interface Transport {
    count: number;
    totalMiles: number;
    totalLunches: number;
    deliveries: Delivery[];
}
