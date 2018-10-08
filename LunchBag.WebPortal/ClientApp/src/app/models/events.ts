import { EventView } from './event-view';
import { EventSentiment } from './event-sentiment';
import { EventLocation } from './event-location';

export interface Events {
    id: string;
    eventName: string;
    eventLocations: EventLocation[];
    eventSentiments: EventSentiment[];
    eventView: EventView;
    isEventActive: boolean;
    lastNote: string;
    goal: number;
    date: Date;
    eTag: string;
}
