﻿using System.Collections.Generic;
using UnityEngine;

public class EntityReferencePropertyManager : MonoBehaviour {
    public class EntityReferenceLine : MonoBehaviour {
        public int i;
        public Entity sourceEntity;
        public Entity targetEntity;
        private LineRenderer line;

        void Start() {
            if (sourceEntity == null || targetEntity == null) {
                return;
            }
            Color color = ColorI(i);
            line = gameObject.AddComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.1f;
            line.material = _lineMaterial;
            line.startColor = line.endColor = color;
            UpdatePositions();
        }

        public void UpdatePositions() {
            line.SetPosition(0, sourceEntity.PositionInEditor());
            line.SetPosition(1, targetEntity.PositionInEditor());
        }
    }


    private static Entity currentEntity;
    private static List<Entity> targetEntities = new List<Entity>();
    private static HashSet<Entity> entitiesToClear = new HashSet<Entity>();
    private static Entity behaviorTarget;
    private static int currentTargetEntityI = -1;

    private static Material _lineMaterial;
    public Material lineMaterial;

    private void Clear() {
        targetEntities.Clear();
        currentEntity = null;
        behaviorTarget = null;
        currentTargetEntityI = -1;
    }

    public static void Reset(Entity entity) {
        foreach (Entity clear in entitiesToClear) {
            if (clear != null && clear != entity) {
                clear.SetHighlight(Color.clear);
            }
        }
        entitiesToClear.Clear();
        entitiesToClear.UnionWith(targetEntities);

        targetEntities.Clear();
        currentTargetEntityI = -1;

        if (currentEntity != entity) {
            // entity deselected
            currentEntity?.SetHighlight(Color.clear);
            if (entity != null) {
                // entity selected
                entity.SetHighlight(Color.white);
                EntityPreviewManager.AddEntity(entity); // refresh
            }
        }
        currentEntity = entity;
        behaviorTarget = null;
    }

    // TODO: delete this when it is no longer needed
    public static Entity CurrentEntity() => currentEntity;

    public static void Next(Entity entity) {
        int existingIndex = targetEntities.IndexOf(entity); // TODO: not efficient
        if (existingIndex != -1) {
            currentTargetEntityI = existingIndex;
            return;
        }
        targetEntities.Add(entity);
        entitiesToClear.Remove(entity);
        currentTargetEntityI = targetEntities.Count - 1;
        entity?.SetHighlight(GetColor());
    }

    public static void SetBehaviorTarget(Entity entity) {
        behaviorTarget = entity;
    }

    public static Color GetColor() {
        if (targetEntities[currentTargetEntityI] == currentEntity
                || targetEntities[currentTargetEntityI] == null) {
            return Color.white;
        }
        return ColorI(currentTargetEntityI);
    }

    private static Color ColorI(int i) => Color.HSVToRGB((i * .618f) % 1.0f, 0.8f, 1.0f);

    public static string GetName(GUIStringSet s) {
        Entity entity = targetEntities[currentTargetEntityI];
        if (entity == null) {
            return s.EntityRefNone;
        } else if (entity == currentEntity) {
            return s.EntityRefSelf;
        } else if (entity == behaviorTarget) {
            return s.EntityRefTarget;
        } else {
            return entity.ToString(s);
        }
    }

    void Awake() {
        _lineMaterial = lineMaterial;
        Clear();
    }

    void OnDestroy() {
        Clear();
    }

    void Update() {
        bool updateTargets = false;
        if (currentEntity == null && transform.childCount != 0) {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            return; // no lines for multiple selection
        } else if (transform.childCount != targetEntities.Count) {
            updateTargets = true;
        } else {
            foreach (Transform child in transform) {
                EntityReferenceLine line = child.GetComponent<EntityReferenceLine>();
                if (targetEntities[line.i] != line.targetEntity || currentEntity != line.sourceEntity) {
                    updateTargets = true;
                    break;
                }
            }
        }

        if (updateTargets) {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < targetEntities.Count; i++) {
                GameObject lineObject = new GameObject();
                lineObject.transform.parent = transform;
                EntityReferenceLine line = lineObject.AddComponent<EntityReferenceLine>();
                line.i = i;
                line.sourceEntity = currentEntity;
                line.targetEntity = targetEntities[i];
            }
        } else if (currentEntity != null) {
            foreach (Transform child in transform) {
                child.GetComponent<EntityReferenceLine>().UpdatePositions();
            }
        }
    }
}
