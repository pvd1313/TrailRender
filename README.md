Test task for Gaijin Entertainment, project Age of Water

# Task - implement wide bullet smoke trail via mesh which is build as bullet is flying.
Each new mesh segment is added as bullet is passed through fixed length (segmentLength). Each segment must be rotating towards dynamic camera. Setting of UV, vertex colors is not required. Pay attention to memory data management, object count and draw calls. Usage of Unity built-in TrailRenderer is prohibited, the task is in implementing same component from scratch. To draw built mesh MeshRenderer must be used. Implement solution and propose optimization ideas in case of many trails.

# Result
Task completion took 10 hours. 

Created scripts are located at Assets/Scripts/TrailRendering.

Build is in folder Build, can be run under Windows x64.

Optimization ideas:
- As we know bullet speed, we can skip attempts of new segment adding. For example, if bullet is flying with speed 60u/s, framerate is 60fps, segment length is 10u, then new segments can be added each 10th frame, can safe resources for distance compution and RAM reading (location of last segment).
- Should be removed line Schedule().Complete() related to job of mesh building, in good practices mesh building should be invoked after receiving data of all bullet positions and completed just before rendering, to maximize cpu resources utilization.
- Geometry shader can be used to build required mesh (converting dots to squares). Might greatly optimize solution, as less data will be transferred to gpu.
- Trail drawing can be implemented via instancing. For each trail start point, start velocity and passed time can be provided. These data is enough to compute trajectory and build mesh entirely on gpu.
- Far trails can be skipped in case of "Many trails", or topology can be optimized with differend buffer / renderer.
