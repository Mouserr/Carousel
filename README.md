# Carousel

NGUI provides great tools for scrolling ui content. 
But sometimes it's necessary to scroll 3d objects. For example 3d models of characters.

This pack of scripts could help with developing some kind of 3d scrolling controls.

### Classes:

**AbstractCarousel** - defines main interface for sort objects and initially moving them.

**InertalMotor** - class that work with implementations of IMoveable interface 
  and produce emulation of animated and inertial movements.
  
**CarouselCenterOnChild** - produce functionality of centering view on one of the scrolling objects

**LineStretchCarousel** - carousel implementation in which all objects stretches on one line.

**CyclicCarousel3d** - carousel implementation in which objects arranged in a cyclic sequence that looks like a stadion field:
<pre>
  @
@   @
@   @
@   @
@   @
  @    - centered object
</pre>
