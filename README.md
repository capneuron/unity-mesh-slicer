# unity-mesh-slicer
This project is inspired by the "Blade Mode" in a video game: Metal Gear Rising, where the player could slice the objects freely.

We implement the mesh slicer that can "slice" from any angle.

![image](https://github.com/capneuron/unity-mesh-slicer/blob/master/Image/5.png?raw=true)

## Instructions
In the example program we provide, you can

- TODO: camera moving instructions.
- **Left mouse button**: Slice the objects.
    - Press down, move the cursor across the object, then release the button.
- **Space**: Blow wind to the objects.

## Slicing Implementation

### Generate new vertices and triangles
To perform the slicing, we will use a plane, defined by a point and a normal vector, to cut the mesh. First, we want to find all the triangles that intersect with the plane. To do this, we test every line in every triangle to find the triangles we need and just do some math to find the intersection points.

Once we have the triangles that intersect with the plane, we add the intersection points to the vertices array. New Triangles will also be generated.

![image](https://github.com/capneuron/unity-mesh-slicer/blob/master/Image/fig3.png?raw=true)

### Split the mesh
The mesh will be cut into two parts. For every triangles and vertices that are originally from the mesh, we decide which part they are from and put them in different vertices arrays and triangles arrays accordingly.(along with their uvs and normals)

For the new triangles generated, we also decide which part they belong to.

For the new vertices generated we put them in both parts because these vertices form the intersection surface and are shared by both parts.

### Create the intersection surface
- Choose a random vertex "root" that belongs to the intersection surface.
- Iterate every triangles that have 2 vertices on the intersection surface, draw a triangle with "root" vertex and these two vertices.
    - Trick here: assume we have triangles *a-b-c* where a and b are the vertices on the intersection surface, the new triangle we need to generate is b-a-root so that the order of the vertices in the triangles is correct.
- Apply new material for the intersection surface.(using submesh)

![image](https://github.com/capneuron/unity-mesh-slicer/blob/master/Image/fig1.jpg?raw=true)

### Generate New Gameobject and Copy Other Component
- Instantiate two new Gameobject and copy components from the original object
- Done!
- The slicing of multiple objects is performed in coroutines.

![image](https://github.com/capneuron/unity-mesh-slicer/blob/master/Image/example.gif?raw=true)

## Other Logic Implementation
TODO

## Fun Fact

![image](https://github.com/capneuron/unity-mesh-slicer/blob/master/Image/cakeSlicing.gif?raw=true)

## Future work / Known issues
- Need to figure out how to decide the uvs(texture coordinates) for the vertices on the intersection surface.
- The number of vertices surges when performing the slicing repeatedly, which cause some performance issue.