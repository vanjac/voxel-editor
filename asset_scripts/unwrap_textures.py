import bpy
import os

in_path = "C:/Users/j/code/voxel-editor/Assets/Resources/GameAssets/Models_src"
out_path = "C:/Users/j/code/voxel-editor/Assets/Resources/GameAssets/Models"

for root, dirs, files in os.walk(in_path):
    for f in files:
        if f.endswith('.meta'): continue
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete()

        bpy.ops.import_scene.fbx(filepath=os.path.join(in_path, f))


        for obj in bpy.data.objects:
            bpy.context.view_layer.objects.active = obj
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.uv.cube_project()
            bpy.ops.object.mode_set(mode='OBJECT')

        bpy.ops.object.select_all(action='SELECT')
        out_file = os.path.join(out_path, os.path.splitext(f)[0] + '.obj')
        bpy.ops.export_scene.obj(filepath=out_file, use_materials=False, use_triangles=True)
