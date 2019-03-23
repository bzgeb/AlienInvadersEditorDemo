using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AlienInvadersWindow : EditorWindow
{
    private string m_MyString;
    private bool m_GroupEnabled;
    private bool m_IsBulletInFlight;
    private float m_BulletSpeed = 1;
    private float m_EnemySpeed = 0.05f;
    private float m_EnemySpeedAccumulator;
    private float m_EnemyDirection = -1;

    private Texture m_PlayerTexture;
    private Rect m_PlayerPositionRect;
    private Texture m_PlayerBulletTexture;
    private Rect m_BulletPositionRect;
    private Texture m_EnemyTexture;
    private List<Rect> m_EnemyPositionRects = new List<Rect>();
    private const int c_NUM_ENEMIES = 10;

    [MenuItem("Window/Custom/Alien Invaders %#&0", false, 0)]
    private static void ShowInvaders()
    {
        GetWindow<AlienInvadersWindow>(true);
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Settings", EditorStyles.boldLabel);
        m_GroupEnabled = EditorGUILayout.BeginToggleGroup("Unlock Settings", m_GroupEnabled);
        m_IsBulletInFlight = EditorGUILayout.Toggle("Is Bullet In Flight", m_IsBulletInFlight);
        m_BulletSpeed = EditorGUILayout.Slider("Player Speed", m_BulletSpeed, -3, 3);

        m_PlayerPositionRect = EditorGUILayout.RectField("Player Position", m_PlayerPositionRect);
        EditorGUI.BeginChangeCheck();
        m_PlayerTexture =
            EditorGUILayout.ObjectField("Player Texture", m_PlayerTexture, typeof(Texture2D), false) as Texture2D;
        if (EditorGUI.EndChangeCheck() && m_PlayerTexture != null)
        {
            m_PlayerPositionRect.width = m_PlayerTexture.width;
            m_PlayerPositionRect.height = m_PlayerTexture.height;

            m_PlayerPositionRect.x = position.width / 2;
            m_PlayerPositionRect.y = position.height - m_PlayerPositionRect.height;
        }

        m_BulletPositionRect = EditorGUILayout.RectField("Bullet Position", m_BulletPositionRect);
        EditorGUI.BeginChangeCheck();
        m_PlayerBulletTexture =
            EditorGUILayout.ObjectField("Bullet Texture", m_PlayerBulletTexture, typeof(Texture2D), false) as Texture2D;
        if (EditorGUI.EndChangeCheck() && m_PlayerBulletTexture != null)
        {
            m_BulletPositionRect.width = m_PlayerBulletTexture.width;
            m_BulletPositionRect.height = m_PlayerBulletTexture.height;
        }

//        m_EnemyPositionRect = EditorGUILayout.RectField("Enemy Position", m_EnemyPositionRect);
        EditorGUI.BeginChangeCheck();
        m_EnemyTexture =
            EditorGUILayout.ObjectField("Enemy Texture", m_EnemyTexture, typeof(Texture2D), false) as Texture2D;

        if (GUILayout.Button("Reset Game"))
        {
            m_EnemyPositionRects = new List<Rect>(c_NUM_ENEMIES);
            for (int i = 0; i < c_NUM_ENEMIES; ++i)
            {
                var newEnemy = new Rect();

                newEnemy.width = m_EnemyTexture.width;
                newEnemy.height = m_EnemyTexture.height;

                newEnemy.x = position.width - (i * (newEnemy.width + 10));
                newEnemy.y = 450;

                m_EnemyPositionRects.Add(newEnemy);
            }

            m_PlayerPositionRect.x = position.width / 2;
            m_PlayerPositionRect.y = position.height - m_PlayerPositionRect.height;
            m_BulletPositionRect.y = -100;
        }

        UpdateGame();
        DrawGame();
    }

    private void UpdateGame()
    {
        var evt = Event.current;
        if (evt.isKey)
        {
            if (evt.keyCode == KeyCode.D)
            {
                m_PlayerPositionRect.x += 1;
                evt.Use();
            }
            else if (evt.keyCode == KeyCode.A)
            {
                m_PlayerPositionRect.x -= 1;
                evt.Use();
            }

            if (evt.keyCode == KeyCode.W && evt.type == EventType.KeyDown)
            {
                m_BulletPositionRect.x = m_PlayerPositionRect.x + (m_PlayerPositionRect.width / 2f) -
                                         (m_BulletPositionRect.width / 2f);
                m_BulletPositionRect.y = m_PlayerPositionRect.y;
                m_IsBulletInFlight = true;
                evt.Use();
            }
        }

        // Use every event?
//        if (this == focusedWindow && evt.type != EventType.Used && evt.type != EventType.Layout &&
//            evt.type != EventType.Repaint)
//        {
//            evt.Use();
//        }

        if (m_IsBulletInFlight)
        {
            m_BulletPositionRect.y -= m_BulletSpeed;

            if (m_BulletPositionRect.y < 0)
            {
                m_IsBulletInFlight = false;
                m_BulletPositionRect.y = -100;
            }

            for (int i = 0; i < c_NUM_ENEMIES; ++i)
            {
                var enemyRect = m_EnemyPositionRects[i];
                if (m_BulletPositionRect.Overlaps(enemyRect))
                {
                    m_IsBulletInFlight = false;
                    m_BulletPositionRect.y = -100;
                    m_EnemyPositionRects[i] = new Rect(-100, -100, 0, 0);
                }
            }
        }

        UpdateEnemies();
    }

    private void DrawGame()
    {
        if (m_PlayerTexture != null)
        {
            GUI.DrawTexture(m_PlayerPositionRect, m_PlayerTexture);
        }

        if (m_PlayerBulletTexture != null)
        {
            GUI.DrawTexture(m_BulletPositionRect, m_PlayerBulletTexture);
        }

        if (m_EnemyTexture != null)
        {
            foreach (var enemy in m_EnemyPositionRects)
            {
                GUI.DrawTexture(enemy, m_EnemyTexture);
            }
        }

        Repaint();
    }

    private void UpdateEnemies()
    {
        if (m_EnemyPositionRects.Count != 10)
        {
            return;
        }

        m_EnemySpeedAccumulator += m_EnemySpeed;
        if (m_EnemySpeedAccumulator > 20)
        {
            for (int i = 0; i < c_NUM_ENEMIES; ++i)
            {
                var oldRect = m_EnemyPositionRects[i];
                var newRect = new Rect(oldRect.x + m_EnemySpeedAccumulator * m_EnemyDirection, oldRect.y, oldRect.width,
                    oldRect.height);
                m_EnemyPositionRects[i] = newRect;
            }

            if (m_EnemyPositionRects[9].x <= 0f && m_EnemyPositionRects[9].x > -100f)
            {
                m_EnemyDirection *= -1;

                for (int i = 0; i < c_NUM_ENEMIES; ++i)
                {
                    var oldRect = m_EnemyPositionRects[i];
                    var newRect = new Rect(oldRect.x, oldRect.y + 64, oldRect.width,
                        oldRect.height);
                    m_EnemyPositionRects[i] = newRect;
                }
            }
            else if (m_EnemyPositionRects[0].x >= position.width)
            {
                m_EnemyDirection *= -1f;

                for (int i = 0; i < c_NUM_ENEMIES; ++i)
                {
                    var oldRect = m_EnemyPositionRects[i];
                    var newRect = new Rect(oldRect.x, oldRect.y + 64, oldRect.width,
                        oldRect.height);
                    m_EnemyPositionRects[i] = newRect;
                }
            }

            m_EnemySpeedAccumulator = 0;
        }
    }
}