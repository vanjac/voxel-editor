import sys
import os
from PIL import Image
from PIL import ImageChops

base_dir = sys.argv[1]
for filename in os.listdir(base_dir):
    if not filename.endswith(".png"):
        continue
    print(filename)
    filepath = os.path.join(base_dir, filename)
    im = Image.open(filepath)
    im = im.convert("RGBA")
    source = list(im.split())
    R, G, B, A = 0, 1, 2, 3
    source[R] = ImageChops.invert(source[R])
    source[G] = ImageChops.invert(source[G])
    source[B] = ImageChops.invert(source[B])
    im = Image.merge(im.mode, source)
    im.save(filepath)
