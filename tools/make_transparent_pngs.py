from collections import deque
from pathlib import Path

from PIL import Image


INPUTS = [
    Path(r"F:\unity\AI photo\chuansongmen1.png"),
    Path(r"F:\unity\AI photo\open chuansongmen.png"),
    Path(r"F:\unity\AI photo\yujin.png"),
    Path(r"F:\unity\AI photo\yujincunchu.png"),
]
OUTPUT_DIR = Path(r"F:\unity\RPG\TransparentAssets")


def is_background_like(r: int, g: int, b: int) -> bool:
    high = max(r, g, b)
    low = min(r, g, b)
    return high >= 218 and low >= 205 and high - low <= 22


def remove_connected_checkerboard(path: Path) -> Path:
    image = Image.open(path).convert("RGBA")
    pixels = image.load()
    width, height = image.size

    visited = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()

    def enqueue(x: int, y: int) -> None:
        idx = y * width + x
        if visited[idx]:
            return
        r, g, b, _ = pixels[x, y]
        if is_background_like(r, g, b):
            visited[idx] = 1
            queue.append((x, y))

    for x in range(width):
        enqueue(x, 0)
        enqueue(x, height - 1)
    for y in range(height):
        enqueue(0, y)
        enqueue(width - 1, y)

    while queue:
        x, y = queue.popleft()
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if 0 <= nx < width and 0 <= ny < height:
                enqueue(nx, ny)

    out = Image.new("RGBA", image.size)
    out_pixels = out.load()
    transparent_count = 0
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if visited[y * width + x]:
                out_pixels[x, y] = (r, g, b, 0)
                transparent_count += 1
            else:
                out_pixels[x, y] = (r, g, b, a)

    OUTPUT_DIR.mkdir(exist_ok=True)
    output = OUTPUT_DIR / f"{path.stem}_transparent.png"
    out.save(output)
    print(f"{output} | {width}x{height} | transparent pixels: {transparent_count}")
    return output


def main() -> None:
    for path in INPUTS:
        remove_connected_checkerboard(path)


if __name__ == "__main__":
    main()
