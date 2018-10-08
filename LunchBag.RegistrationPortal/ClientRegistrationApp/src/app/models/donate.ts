import { Donor } from './donor';
export interface Donate {
    donor: Donor;
    type: string;
    amount: number;
}
