---
modified:
  - 2026-03-03 20:27
---
The animation system is built around the `AnimationStep` interface:
```typescript
interface AnimationStep {
    play():void
    onFinish:Function | undefined
}
```
In other words, we expect an implementation of the `AnimationStep` interface to do something when we call `play()`, and at some point after calling `play()` we expect to be able to call an `onFinish` callback function (if it exists).

Notably, the built-in Konva Tween class already adheres to this interface because it has a `play()` method and an `onFinish` property!

We also have implemented some larger wrapper animation classes that make it easy to handle really common types of animations, such as playing a sequence of animations one after another or playing multiple animations simultaneously. By mixing and matching these, you should be able to very quickly assemble complex animations from smaller components. And the `AnimationStep` interface makes it very easy for these to combine in very complex ways!

When implementing your own implementation of the `AnimationStep`, don't create any functionality that relies heavily on setting its own `onFinish` callback. This callback is designed to be set by the thing that calls it (for example, the AnimationSequence needs to call the next step in the sequence when the first one finishes; and the main game loop needs to know when an Animation is complete in order to call the next one).

Here is some documentation on some of the specific larger animation utility classes:

# Animation component utilities
## AnimationSequence
Plays every AnimationStep provided to its `steps` property in order, waiting for each one to finish before calling the next one.
### Constructor args:
```typescript
steps: Array<AnimationStep>
```

## SimultaneousAnimation
Plays every AnimationStep provided to its `steps` property at the same time. Once *every* step has completed, will fire the `onFinish` callback.
### Constructor args:
```typescript
steps: Array<AnimationStep>
```

## TweenFromCurrent
This class will generate a brand new Konva.Tween immediately before playing it. This is important because the starting values of a Konva.Tween are set to whatever they were when the Tween was created, so this is especially useful in combination with `AnimationSequence`.

This class also allows for dynamic assignment of properties by setting the value of any of the normal `TweenConfig` properties to a function which will return a valid value. Most of the time the easiest way to do this will be with an anonymous function like:
```typescript
property: () => {return something}
// or
property: () => something

y: () => node.position().y + 100
```

You can mix and match functions and non-functions--non-functions will be evaluated at the time that the `TweenFromCurrent` is originally created like normal, not on play like when you pass a function.

**IMPORTANT NOTE:** The implementation will evaluate the return value of any Function parameters. This means that if you want the actual *value* of the `TweenConfig` property to be a function, you need to wrap it in an anonymous function:
```typescript
easing: () => Easings.EaseIn
```
(I think this mostly matters for the `easing` property)

## AnimationPause
This just waits for `duration` milliseconds and then calls `onFinish`.
### Constructor args:
```typescript
duration: number
```

# Specific animations

## Leap
Animate a character (or other thing) jumping in an arc from its starting position to the destination. 

The `destination` parameter can either be another Node or a Vector2d. If it is a Vector2d, it will jump directly to that position. If it is a Node, it will jump to a spot in front of that Node.

`duration` is in seconds and includes leaping there and back, but not the `landingAnimation` (if any).

`landingAnimation` is another `AnimationStep` to play once the `leapingNode` has landed at its destination but before leaping back. This could allow you to leap to a target and show a hit particle effect for example.
### Constructor args
```typescript
{

    leapingNode:Konva.Node,

    destination:Konva.Node | Konva.Vector2d,

    jumpHeight?:number, // default = 50

    leapDuration?:number, // default = 1

    landingAnimation?:AnimationStep // default = new AnimationPause()  <- (0 duration)

}
```

### Example
```typescript
let leapToNode:LeapAnimation = new LeapAnimation({
	leapingNode: circle,
	destination: otherCircle,
	landingAnimation: new AnimationPause(1000)
})
    
let leapToPosition:LeapAnimation = new LeapAnimation({
	leapingNode: circle,
	destination: {x: circle.position().x + 300, y: circle.position().y - 200},
	landingAnimation: new AnimationPause(1000)
})
```