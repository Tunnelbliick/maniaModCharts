using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using OpenTK;
using storyboard.scriptslibrary.maniaModCharts.Draw;
using storyboard.scriptslibrary.maniaModCharts.effects;
using StorybrewCommon.Animations;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{

    public enum PathType
    {
        line,
        bezier
    }

    public class DrawInstance : Drawer
    {

        public double starttime = 0;
        public double easetime = 0;
        public OsbEasing easing;
        public Playfield playfieldInstance;
        public double updatesPerSecond = 2;
        public double fadeInTime = 50;
        public double fadeOutTime = 10;
        public bool rotateToFaceReceptor = true;
        public double iterationLength = 1000 / 2;

        public bool hideHolds = false;
        public bool hideNormalNotes = false;
        public Dictionary<ColumnType, List<OsbSprite>> pathWaySprites = new Dictionary<ColumnType, List<OsbSprite>>();

        public Dictionary<double, double> updatesPerSecondDictionary = new Dictionary<double, double>();


        public Dictionary<ColumnType, List<Anchor>> notePathByColumn = new Dictionary<ColumnType, List<Anchor>>();

        public DrawInstance InitializeDrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor)
        {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;
            this.iterationLength = 1000 / updatesPerSecond;
            this.changeUpdateRate(starttime, updatesPerSecond);

            return this;

        }

        public DrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor)
        {

            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;
            this.iterationLength = 1000 / updatesPerSecond;
            this.changeUpdateRate(starttime, updatesPerSecond);

        }


        public DrawInstance(Playfield playfieldInstance, double starttime, double easetime, double updatesPerSecond, OsbEasing easing, bool rotateToFaceReceptor, double fadeInTime, double fadeOutTime)
        {
            this.starttime = starttime;
            this.easetime = easetime;
            this.easing = easing;
            this.playfieldInstance = playfieldInstance;
            this.updatesPerSecond = updatesPerSecond;
            this.rotateToFaceReceptor = rotateToFaceReceptor;
            this.fadeInTime = fadeInTime;
            this.fadeOutTime = fadeOutTime;
            this.iterationLength = 1000 / updatesPerSecond;
            this.changeUpdateRate(starttime, updatesPerSecond);
        }

        public void drawNotesStutteredByOriginToReceptor(double duration, bool renderReceptor = true)
        {
            Stutter.drawNotesStutteredByOriginToReceptor(this, duration, renderReceptor);
        }

        public void drawNotesByOriginToReceptor(double duration, bool renderReceptor = true)
        {
            OriginToReceptor.drawNotesByOriginToReceptor(this, duration, renderReceptor);
        }

        public void drawNotesByAnchors(double duration, PathType type = PathType.line)
        {
            ByAnchors.drawNotesByAnchors(this, duration, type);
        }

        public void drawPath(double starttime, double endtime, StoryboardLayer layer, string spritePath, PathType type, int precision, int updatesPerSecond = 3)
        {
            PathWay.DrawPath(this, starttime, endtime, layer, spritePath, type, precision, updatesPerSecond);
        }

        public void drawViaEquation(double duration, EquationFunction noteFunction, bool renderReceptor = true) {
            ByEquation.drawViaEquation(this, duration, noteFunction, renderReceptor);
        }

        public List<Vector2> GetPathAnchorVectors(List<Anchor> notePath, double currentTime)
        {
            List<Vector2> points = new List<Vector2>();
            foreach (Anchor noteAnchor in notePath)
            {
                points.Add(noteAnchor.getPositionAt(currentTime));
            }

            return points;
        }

        public void addAnchor(ColumnType column, Vector2 position, bool debug, StoryboardLayer debugLayer)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    if (notePathByColumn.ContainsKey(currentColumn) == false)
                    {

                        notePathByColumn.Add(currentColumn, new List<Anchor>());

                    }

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    //if debug add a sprite for the position of the vector
                    Anchor pathPoint = new Anchor(0, starttime, currentColumn, position, position, debug, debugLayer);
                    notePath.Add(pathPoint);

                    notePathByColumn[currentColumn] = notePath;

                }
            }
            else
            {

                if (notePathByColumn.ContainsKey(column) == false)
                {

                    notePathByColumn.Add(column, new List<Anchor>());

                }

                List<Anchor> notePath = notePathByColumn[column];

                //if debug add a sprite for the position of the vector
                Anchor pathPoint = new Anchor(0, starttime, column, position, position, debug, debugLayer);
                notePath.Add(pathPoint);

                notePathByColumn[column] = notePath;
            }
        }

        // This adds an anchor relative to the current position of the column
        public String addRelativeAnchor(ColumnType column, double starttime, Vector2 relativeOffset, bool debug, StoryboardLayer debugLayer)
        {

            String debugstring = "";

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumnType in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumnType == ColumnType.all)
                        continue;

                    debugstring = addRelative(currentColumnType, starttime, relativeOffset, debug, debugLayer);
                }
            }
            else
            {
                debugstring = addRelative(column, starttime, relativeOffset, debug, debugLayer);
            }

            return debugstring;
        }

        public String addRelativeAnchorList(ColumnType column, double starttime, List<Vector2> relativeOffset, bool debug, StoryboardLayer debugLayer)
        {

            String debugstring = "";

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumnType in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumnType == ColumnType.all)
                        continue;

                    debugstring = addListRelative(currentColumnType, starttime, relativeOffset, debug, debugLayer);
                }
            }
            else
            {
                debugstring = addListRelative(column, starttime, relativeOffset, debug, debugLayer);
            }

            return debugstring;
        }

        private String addRelative(ColumnType column, double starttime, Vector2 relativeOffset, bool debug, StoryboardLayer debugLayer)
        {

            String debugString = "";

            if (this.notePathByColumn.ContainsKey(column) == false)
            {
                this.notePathByColumn.Add(column, new List<Anchor>());
            }

            List<Anchor> notePath = this.notePathByColumn[column];
            Column currentColumn = this.playfieldInstance.columns[column];
            Vector2 originPosition = currentColumn.getOriginPosition(starttime);
            Vector2 receptorPosition = currentColumn.getReceptorPositionForNotes(starttime);
            int index = 0;

            float blend = 1.0f / (notePath.Count + 1);

            foreach (Anchor noteAnchor in notePath)
            {

                float currentBlend = blend * index;
                debugString += $"{blend}, {index}, {currentBlend}, {notePath.Count}\n";

                Vector2 offsetPosition = noteAnchor.position;
                Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                offsetPosition.Y = lerpPosition.Y;

                index++;

                noteAnchor.MoveAnchor(starttime, offsetPosition);

            }

            Vector2 lerpPositionForNewAnchor = Vector2.Lerp(originPosition, receptorPosition, 1);
            Vector2 offsetPositionForNewAnchor = Vector2.Add(lerpPositionForNewAnchor, relativeOffset);

            //if debug add a sprite for the position of the vector
            Anchor pathPoint = new Anchor(0, starttime, column, offsetPositionForNewAnchor, relativeOffset, debug, debugLayer);
            notePath.Add(pathPoint);

            this.notePathByColumn[column] = notePath;

            return debugString;
        }

        private String addListRelative(ColumnType column, double starttime, List<Vector2> relativeOffsets, bool debug, StoryboardLayer debugLayer)
        {

            String debugString = "";

            if (this.notePathByColumn.ContainsKey(column) == false)
            {
                this.notePathByColumn.Add(column, new List<Anchor>());
            }

            List<Anchor> notePath = this.notePathByColumn[column];
            Column currentColumn = this.playfieldInstance.columns[column];
            Vector2 originPosition = currentColumn.getOriginPosition(starttime);
            Vector2 receptorPosition = currentColumn.getReceptorPosition(starttime);

            int index = 0;

            float blend = 1.0f / (notePath.Count + relativeOffsets.Count - 1);

            foreach (Anchor noteAnchor in notePath)
            {

                float currentBlend = blend * index;
                //debugString += $"{blend}, {index}, {currentBlend}, {notePath.Count}\n";
                debugString += $"{currentColumn}, {originPosition}, {receptorPosition}, {notePath.Count}\n";

                Vector2 offsetPosition = noteAnchor.position;
                Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                offsetPosition.Y = lerpPosition.Y;

                index++;
                noteAnchor.MoveAnchor(starttime, offsetPosition);

            }

            foreach (Vector2 offset in relativeOffsets)
            {

                float currentBlend = blend * index;

                Vector2 lerpPositionForNewAnchor = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                Vector2 offsetPositionForNewAnchor = Vector2.Add(lerpPositionForNewAnchor, offset);
                debugString += $"{currentColumn}, {offsetPositionForNewAnchor}, {lerpPositionForNewAnchor}, {notePath.Count}\n";

                //if debug add a sprite for the position of the vector
                Anchor pathPoint = new Anchor(0, starttime, column, offsetPositionForNewAnchor, offset, debug, debugLayer);
                notePath.Add(pathPoint);

                index++;
            }

            this.notePathByColumn[column] = notePath;

            return debugString;
        }

        public void ManipulateAnchorRelative(int index, double starttime, double transitionTime, Vector2 newPosition, OsbEasing easing, ColumnType column = ColumnType.all)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Anchor pathPoint = notePath[index];

                    Vector2 offset = Vector2.Add(pathPoint.getPositionAt(starttime), newPosition);

                    pathPoint.ManipulatePosition(starttime, transitionTime, easing, offset);

                    notePath[index] = pathPoint;

                }
            }
            else
            {

                List<Anchor> notePath = notePathByColumn[column];

                Anchor pathPoint = notePath[index];

                Vector2 offset = Vector2.Add(pathPoint.getPositionAt(starttime), newPosition);

                pathPoint.ManipulatePosition(starttime, transitionTime, easing, offset);

                notePath[index] = pathPoint;

            }

        }

        public double ResetAnchors(double starttime, double transitionTime, OsbEasing easing, ColumnType column = ColumnType.all)
        {
            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Column selectedColumn = this.playfieldInstance.columns[currentColumn];

                    Vector2 originPosition = selectedColumn.getOriginPosition(starttime);
                    Vector2 receptorPosition = selectedColumn.getReceptorPosition(starttime);

                    float blend = 1.0f / (notePath.Count - 1);
                    int index = 0;
                    foreach (Anchor noteAnchor in notePath)
                    {

                        float currentBlend = blend * index;

                        Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                        noteAnchor.ManipulatePosition(starttime, transitionTime, easing, lerpPosition);
                        index++;

                    }

                }
            }
            else
            {

                if (column == ColumnType.all)
                    return starttime + transitionTime;

                List<Anchor> notePath = notePathByColumn[column];

                Column selectedColumn = this.playfieldInstance.columns[column];
                Vector2 originPosition = selectedColumn.getOriginPosition(starttime);
                Vector2 receptorPosition = selectedColumn.getReceptorPosition(starttime);

                float blend = 1.0f / (notePath.Count - 1);
                int index = 0;
                foreach (Anchor noteAnchor in notePath)
                {

                    float currentBlend = blend * index;

                    Vector2 lerpPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);
                    noteAnchor.ManipulatePosition(starttime, transitionTime, easing, lerpPosition);
                    index++;

                }

            }

            return starttime + transitionTime;

        }


        public void ManipulateAnchorAbsolute(int index, double starttime, double transitionTime, Vector2 newPosition, OsbEasing easing, ColumnType column)
        {

            if (column == ColumnType.all)
            {

                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    List<Anchor> notePath = notePathByColumn[currentColumn];

                    Anchor pathPoint = notePath[index];

                    pathPoint.ManipulatePosition(starttime, transitionTime, easing, newPosition);

                    notePath[index] = pathPoint;

                }
            }
            else
            {

                List<Anchor> notePath = notePathByColumn[column];

                Anchor pathPoint = notePath[index];

                pathPoint.ManipulatePosition(starttime, transitionTime, easing, newPosition);

                notePath[index] = pathPoint;

            }

        }

        // TODO figure this shit out!
        public String UpdateAnchors(double starttime, double duration, ColumnType column)
        {
            String debug = "";

            double endtime = starttime + duration;
            double currentTime = starttime;

            while (currentTime <= endtime)
            {
                double localIterationRate = findCurrentUpdateRate(currentTime);

                debug += $"{currentTime}, {localIterationRate}\n";

                if (column == ColumnType.all)
                {

                    foreach (ColumnType type in Enum.GetValues(typeof(ColumnType)))
                    {

                        if (type == ColumnType.all)
                            continue;

                        List<Anchor> notePath = this.notePathByColumn[type];
                        Column currentColumn = this.playfieldInstance.columns[type];
                        Vector2 originPosition = currentColumn.getOriginPosition(currentTime + localIterationRate);
                        Vector2 receptorPosition = currentColumn.getReceptorPositionForNotes(currentTime + localIterationRate);

                        int index = 0;
                        float blend = 1.0f / (notePath.Count - 1);

                        Vector2 direction = Vector2.Normalize(receptorPosition - originPosition); // Direction from origin to receptor
                        Vector2 perpendicular = new Vector2(-direction.Y, direction.X); // Perpendicular to the direction

                        foreach (Anchor noteAnchor in notePath)
                        {
                            float currentBlend = blend * index;

                            // Determine the position along the path
                            Vector2 pathPosition = Vector2.Lerp(originPosition, receptorPosition, currentBlend);

                            // Apply the offset relative to the path's direction
                            Vector2 offsetPosition = noteAnchor.offset;
                            Vector2 adjustedPosition = pathPosition - (offsetPosition.X * perpendicular) + (offsetPosition.Y * direction);

                            noteAnchor.ManipulatePosition(currentTime, localIterationRate, OsbEasing.None, adjustedPosition);

                            index++;
                        }
                    }
                }

                currentTime += localIterationRate;
                currentTime = Math.Round(currentTime);

            }

            return debug;

        }

        public double FadePath(double starttime, double duration, OsbEasing easing, float fade, ColumnType column = ColumnType.all)
        {

            if (column == ColumnType.all)
            {
                foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
                {

                    if (currentColumn == ColumnType.all)
                        continue;

                    foreach (OsbSprite path in this.pathWaySprites[currentColumn])
                    {
                        float currentFade = path.OpacityAt(starttime);
                        path.Fade(easing, starttime, starttime + duration, currentFade, fade);
                    }
                }
            }
            else
            {
                foreach (OsbSprite path in this.pathWaySprites[column])
                {
                    float currentFade = path.OpacityAt(starttime);
                    path.Fade(easing, starttime, starttime + duration, currentFade, fade);
                }
            }

            return starttime + duration;
        }

        public String debugBezier(double starttime, StoryboardLayer layer)
        {

            String debug = "";

            foreach (ColumnType currentColumn in Enum.GetValues(typeof(ColumnType)))
            {

                if (currentColumn == ColumnType.all)
                    continue;

                List<Anchor> notePath = notePathByColumn[currentColumn];
                List<Vector2> points = new List<Vector2>();

                foreach (Anchor noteAnchor in notePath)
                {
                    // points.Add(noteAnchor.sprite.PositionAt(starttime));
                    points.Add(noteAnchor.getPositionAt(starttime));
                }

                const int resolution = 50;
                for (float t = 0; t <= 1; t += 1f / (resolution - 1))
                {
                    Vector2 pointOnCurve = BezierCurve.CalculatePoint(points, t);
                    // Draw or store the point as required
                    debug += $"{pointOnCurve}";
                    OsbSprite sprite = layer.CreateSprite("sb/white1x.png", OsbOrigin.Centre, pointOnCurve);
                    sprite.Fade(starttime, 1);
                    sprite.Fade(starttime + 50000, 0);

                }
            }

            return debug;

        }

        public KeyValuePair<double, EffectInfo> findEffectByReferenceTime(double time)
        {
            KeyValuePair<double, EffectInfo> currentEffect = playfieldInstance.effectReferenceByStartTime
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentEffect;
        }

        public double findCurrentUpdateRate(double time)
        {
            double iterationrate = this.iterationLength;

            KeyValuePair<double, double> currentUpdateRate = updatesPerSecondDictionary
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            if (currentUpdateRate.Value != 0)
            {
                iterationrate = 1000 / currentUpdateRate.Value;
            }

            return Math.Round(iterationrate);
        }

        public FadeEffect findFadeAtTime(double time)
        {

            KeyValuePair<double, FadeEffect> currentFadeEffect = this.playfieldInstance.fadeAtTime
                   .Where(kvp => kvp.Key <= time)
                   .OrderByDescending(kvp => kvp.Key)
                   .FirstOrDefault();

            return currentFadeEffect.Value;
        }

        public void changeUpdateRate(double time, double updatesPerSecond)
        {
            updatesPerSecondDictionary.Add(Math.Max(time - this.easetime, 0), updatesPerSecond);
        }
    }
}