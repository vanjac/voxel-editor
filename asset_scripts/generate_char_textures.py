import cgi
import wand.image

for char_i in range(33, 126):
    svg = """
<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">
  <text x="128" y="200" text-anchor="middle" font-family="Cooper Black" font-size="280"
    fill="black">{}</text>
</svg>
    """.format(cgi.escape(chr(char_i)))
    with wand.image.Image(blob=svg.encode('utf-8'), format='svg') as img:
        filename = '../Assets/GameAssets/Characters/c' + str(char_i).zfill(3) + '.png'
        img.save(filename=filename)
