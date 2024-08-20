# Project Details
I created this as a sample project to go along with a YouTube video around the topic of Simulating Boids in Unity utilizing the Job System and Burst Compiler while remaining free of any ECS related code. I am utilising the new **Unity DOTS Physics** package to handle world collisions between boids and sample spheres.

![boid system](https://github.com/user-attachments/assets/72c9cbe7-1f09-4a1e-b803-7010ee55798e)

There are a lot of 'boid simulation' projects however I fail to see their game readiness in the following aspects:

- No support for Arrival detection
- No support for MonoBehaviour based logic since they are pure ECS
- No support for collision with the Game World

ECS is useful for performance however I don't see it's viability for usage in small scale indie games such as one I am developing.

I have a blog post going into more details of this project: https://codewithajay.com/?p=40

## Scenes

- Performance Testing: This scene has around 2000 units, tested to work around 150 FPS on my machine - the units will go to where you click
- Formation: This scene shows an example of how you can lay out units into various shapes and formations while preserving their collisions and rough shape

![Screenshot 2024-08-21 at 1 24 34 am](https://github.com/user-attachments/assets/b35c688b-3713-43d3-b61a-cc1fec24ad0e)
