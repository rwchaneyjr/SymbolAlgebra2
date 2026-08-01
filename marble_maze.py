import math

from direct.showbase.ShowBase import ShowBase
from direct.gui.OnscreenText import OnscreenText
from panda3d.core import *
from direct.task import Task


MAZE_LAYOUT = [
    "##########",
    "#S       #",
    "# ### ## #",
    "# #    # #",
    "# # ## # #",
    "# # #  # #",
    "# # # ## #",
    "#   #    #",
    "# ####  G#",
    "##########",
]


class MarbleMaze(ShowBase):

    CELL_SIZE = 4
    FLOOR_PADDING = 10
    BOX_MODEL_HALF = 1.0  # models/box spans -1..1 before scaling

    FLOOR_THICK = 0.2
    WALL_HEIGHT = 1.2
    BALL_SCALE = 0.45
    BALL_RADIUS = BALL_SCALE * 0.5
    GOAL_SCALE = 0.8
    GOAL_HALF = GOAL_SCALE * 0.5
    COLLISION_PADDING = 0.15

    FLOOR_Z = 0
    FLOOR_TOP = FLOOR_Z + FLOOR_THICK / 2
    WALL_Z = FLOOR_TOP + WALL_HEIGHT / 2
    BALL_Z = FLOOR_TOP + BALL_RADIUS
    GOAL_Z = FLOOR_TOP + GOAL_HALF

    def __init__(self):
        ShowBase.__init__(self)
        self.disableMouse()
        self.setBackgroundColor(0.45, 0.45, 0.45, 1)
        self.accept("escape", exit)
        self.accept("r", self.reset)
        self.keys = {"w": False, "a": False, "s": False, "d": False}
        for k in self.keys:
            self.accept(k, self.setKey, [k, True])
            self.accept(k + "-up", self.setKey, [k, False])
        self.setupLights()
        self.createLevel()
        self.taskMgr.add(self.update, "update")

    def setupLights(self):
        ambient = AmbientLight("ambient")
        ambient.setColor((0.6, 0.6, 0.6, 1))
        self.render.setLight(self.render.attachNewNode(ambient))
        sun = DirectionalLight("sun")
        sun.setColor((1, 1, 1, 1))
        sun_np = self.render.attachNewNode(sun)
        sun_np.setHpr(-45, -45, 0)
        self.render.setLight(sun_np)

    def apply_solid_color(self, model, r, g, b, a=1):
        model.setTextureOff(TextureStage.getDefault())
        model.setColor(r, g, b, a)

    def cell_center(self, col, row, cols, rows):
        x = -cols * self.CELL_SIZE / 2 + col * self.CELL_SIZE + self.CELL_SIZE / 2
        y = rows * self.CELL_SIZE / 2 - row * self.CELL_SIZE - self.CELL_SIZE / 2
        return x, y

    def circle_vs_wall(self, px, py, wall_cx, wall_cy, half_x, half_y):
        """Circle-vs-AABB test. Returns push-out normal and overlap, or None."""
        radius = self.BALL_RADIUS + self.COLLISION_PADDING

        closest_x = max(wall_cx - half_x, min(px, wall_cx + half_x))
        closest_y = max(wall_cy - half_y, min(py, wall_cy + half_y))

        dx = px - closest_x
        dy = py - closest_y
        dist_sq = dx * dx + dy * dy

        if dist_sq >= radius * radius:
            return None

        if dist_sq > 1e-9:
            dist = math.sqrt(dist_sq)
            return dx / dist, dy / dist, radius - dist

        # Ball center is inside the wall box; push out along the shortest axis.
        left = px - (wall_cx - half_x)
        right = (wall_cx + half_x) - px
        bottom = py - (wall_cy - half_y)
        top = (wall_cy + half_y) - py
        min_pen = min(left, right, bottom, top)

        if min_pen == left:
            return -1.0, 0.0, radius + left
        if min_pen == right:
            return 1.0, 0.0, radius + right
        if min_pen == bottom:
            return 0.0, -1.0, radius + bottom
        return 0.0, 1.0, radius + top

    def collides_at(self, px, py):
        for wall_cx, wall_cy, half_x, half_y in self.wall_bounds:
            if self.circle_vs_wall(px, py, wall_cx, wall_cy, half_x, half_y):
                return True
        return False

    def resolve_collisions(self, pos):
        for _ in range(6):
            moved = False
            for wall_cx, wall_cy, half_x, half_y in self.wall_bounds:
                hit = self.circle_vs_wall(pos.x, pos.y, wall_cx, wall_cy, half_x, half_y)
                if hit:
                    nx, ny, overlap = hit
                    pos.x += nx * overlap
                    pos.y += ny * overlap
                    moved = True
            if not moved:
                break
        return pos

    def wall_half_extents(self, sx, sy):
        return sx * self.BOX_MODEL_HALF, sy * self.BOX_MODEL_HALF

    def make_wall(self, x, y, sx, sy):
        wall = self.loader.loadModel("models/box")
        wall.reparentTo(self.render)
        wall.setScale(sx, sy, self.WALL_HEIGHT)
        wall.setPos(x, y, self.WALL_Z)
        self.apply_solid_color(wall, 0.45, 0.45, 0.45)
        return wall

    def build_maze(self, layout):
        self.walls = []
        self.wall_bounds = []
        start_pos = None
        goal_pos = None
        rows = len(layout)
        cols = len(layout[0])

        for row, line in enumerate(layout):
            for col, ch in enumerate(line):
                x, y = self.cell_center(col, row, cols, rows)
                if ch == "#":
                    self.walls.append(self.make_wall(x, y, self.CELL_SIZE, self.CELL_SIZE))
                    half_x, half_y = self.wall_half_extents(self.CELL_SIZE, self.CELL_SIZE)
                    self.wall_bounds.append((x, y, half_x, half_y))
                elif ch == "S":
                    start_pos = (x + 1.0, y - 1.0)
                elif ch == "G":
                    goal_pos = (x, y)
        return start_pos, goal_pos, cols, rows

    def make_floor(self, width, height):
        floor = self.loader.loadModel("models/box")
        floor.reparentTo(self.render)
        floor.setScale(width + 2, height, self.FLOOR_THICK)
        floor.setPos(-22, -20, self.FLOOR_Z)
        self.apply_solid_color(floor, 0.4, 0.7, 0.4)
        return floor

    def createLevel(self):
        self.maze_cols = len(MAZE_LAYOUT[0])
        self.maze_rows = len(MAZE_LAYOUT)
        maze_width = self.maze_cols * self.CELL_SIZE
        maze_height = self.maze_rows * self.CELL_SIZE
        wall_half, _ = self.wall_half_extents(self.CELL_SIZE, self.CELL_SIZE)
        inner_limit = maze_width / 2 - wall_half - self.BALL_RADIUS - self.COLLISION_PADDING
        self.play_limit = inner_limit

        self.floor = self.make_floor(
            maze_width + self.FLOOR_PADDING,
            maze_height + self.FLOOR_PADDING,
        )

        start_pos, goal_pos, cols, rows = self.build_maze(MAZE_LAYOUT)
        self.start_pos = start_pos

        self.ball = self.loader.loadModel("models/smiley")
        self.ball.reparentTo(self.render)
        self.ball.setScale(self.BALL_SCALE)
        self.ball.setPos(start_pos[0], start_pos[1], self.BALL_Z)

        self.goal = self.loader.loadModel("models/box")
        self.goal.reparentTo(self.render)
        self.goal.setScale(self.GOAL_SCALE)
        self.goal.setPos(goal_pos[0], goal_pos[1], self.GOAL_Z)
        self.apply_solid_color(self.goal, 1, 1, 0)

        self.text = OnscreenText(
            text="Reach the yellow cube!  WASD = move  R = restart",
            pos=(-1.25, 0.9), scale=0.055, align=TextNode.ALeft,
        )

        self.camera.setPos(0, -maze_height * 1.1, maze_height * 0.85)
        self.camera.lookAt(0, 0, 0)

    def setKey(self, key, value):
        self.keys[key] = value

    def reset(self):
        self.ball.setPos(self.start_pos[0], self.start_pos[1], self.BALL_Z)
        self.text.setText("Reach the yellow cube!  WASD = move  R = restart")

    def try_move(self, old_pos, new_x, new_y):
        pos = Point3(old_pos.x, old_pos.y, self.BALL_Z)

        pos.x = new_x
        pos = self.resolve_collisions(pos)

        pos.y = new_y
        pos = self.resolve_collisions(pos)

        pos.z = self.BALL_Z
        return pos

    def update(self, task):
        dt = globalClock.getDt()
        old_pos = self.ball.getPos()
        speed = 8
        dx = dy = 0
        if self.keys["w"]:
            dy += speed * dt
        if self.keys["s"]:
            dy -= speed * dt
        if self.keys["a"]:
            dx -= speed * dt
        if self.keys["d"]:
            dx += speed * dt
        new_x = max(-self.play_limit, min(self.play_limit, old_pos.x + dx))
        new_y = max(-self.play_limit, min(self.play_limit, old_pos.y + dy))
        self.ball.setPos(self.try_move(old_pos, new_x, new_y))
        if (self.goal.getPos() - self.ball.getPos()).length() < 1.5:
            self.text.setText("YOU WIN! Press R to play again")
        return Task.cont


game = MarbleMaze()
game.run()
