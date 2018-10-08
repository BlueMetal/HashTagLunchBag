import { trigger,state,style,transition,animate,keyframes } from '@angular/animations';

export let flyingHeart = trigger('flyingHeart', [
    state('initialSt', style({
        transform: 'scale(0.5)',
        opacity:0.05
    })),
    state('right', style({
        transform: 'scale(0.5)',
        opacity:0.0
    })),
    state('middle', style({
        transform: 'scale(0.5)',
        opacity:0.0
    })),
    state('left', style({
        transform: 'scale(0.5)',
        opacity:0.0
    })),

    transition('initialSt => right', animate(1000, keyframes([
        style({
            transform:'translate(100px,-100px)',
            opacity:0.1,
            offset:0
        }),
        style({
            transform:'translate(225px,-175px)',
            opacity:0.5,
            offset:0.25
        }),
        style({
            transform:'translate(350px,-250px)',
            opacity:1,
            offset:0.5
        }),
        style({
            transform:'translate(475px,-325px)',
            opacity:0.5,
            offset:0.75
        }),
        style({
            transform:'translate(600px,-400px)',
            opacity:0,
            offset:1.0
        }),
    ]))),
    transition('initialSt => middle', animate(1000, keyframes([
        style({
            transform:'translate(5px,-100px)',
            opacity:0.1,
            offset:0
        }),
        style({
            transform:'translate(5px,-175px)',
            opacity:0.5,
            offset:0.25
        }),
        style({
            transform:'translate(5px,-250px)',
            opacity:1,
            offset:0.5
        }),
        style({
            transform:'translate(5px,-325px)',
            opacity:0.5,
            offset:0.75
        }),
        style({
            transform:'translate(5px,-400px)',
            opacity:0,
            offset:1.0
        }),
    ]))),
    transition('initialSt => left', animate(1000, keyframes([
        style({
            transform:'translate(-100px,-100px)',
            opacity:0.1,
            offset:0
        }),
        style({
            transform:'translate(-225px,-175px)',
            opacity:0.5,
            offset:0.25
        }),
        style({
            transform:'translate(-350px,-250px)',
            opacity:1,
            offset:0.5
        }),
        style({
            transform:'translate(-475px,-325px)',
            opacity:0.5,
            offset:0.75
        }),
        style({
            transform:'translate(-600px,-400px)',
            opacity:0,
            offset:1.0
        }),
    ]))),
]);