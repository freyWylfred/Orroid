using Android.Graphics;
using Android.Views;

namespace Orroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        // スキル情報を管理するクラス
        private class SkillInfo
        {
            public CircularGaugeView? Gauge { get; set; }
            public Button? Button { get; set; }
            public float Progress { get; set; } = 0f;
            public string Name { get; set; } = "";
            public int MinDamage { get; set; }
            public int MaxDamage { get; set; }
            public Color NormalColor { get; set; }
            public Color ReadyColor { get; set; }
        }

        private readonly List<SkillInfo> _skills = [];
        private View? _enemyHpBar;
        private View? _enemyContainer;
        private View? _rewardContainer;
        private TextView? _playerHpText;
        private TextView? _enemyCharacter;
        private TextView? _expRewardText;
        private TextView? _goldRewardText;
        private int _enemyHpBarMaxWidth;

        private System.Timers.Timer? _gaugeTimer;
        private System.Timers.Timer? _enemyAttackTimer;
        private int _enemyHp = 100;
        private int _playerHp = 100;
        private const int EnemyMaxHp = 100;
        private const int PlayerMaxHp = 100;
        private const float GaugeIncrement = 0.01f;
        private const int TimerInterval = 50;
        private const int EnemyAttackInterval = 2000; // 敵は2秒ごとに攻撃
        private const float PenaltyAmount = 0.25f;
        private bool _isDefending = false;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            // 敵のUI取得
            _enemyHpBar = FindViewById<View>(Resource.Id.enemyHpBar);
            _enemyCharacter = FindViewById<TextView>(Resource.Id.enemyCharacter);
            _enemyContainer = FindViewById<View>(Resource.Id.enemyContainer);
            _rewardContainer = FindViewById<View>(Resource.Id.rewardContainer);
            _expRewardText = FindViewById<TextView>(Resource.Id.expRewardText);
            _goldRewardText = FindViewById<TextView>(Resource.Id.goldRewardText);

            // プレイヤーのUI取得
            _playerHpText = FindViewById<TextView>(Resource.Id.playerHpText);

            // HPバーの幅を取得（レイアウト完了後）
            _enemyHpBar!.Post(() =>
            {
                _enemyHpBarMaxWidth = _enemyHpBar.Width;
            });

            // スキルの初期化
            InitializeSkills();

            // ゲージタイマーの開始
            StartGaugeTimer();

            // 敵の攻撃タイマー開始
            StartEnemyAttackTimer();
        }

        private void InitializeSkills()
        {
            // 連打切り（高ダメージ・赤系）
            _skills.Add(new SkillInfo
            {
                Gauge = FindViewById<CircularGaugeView>(Resource.Id.gaugeRapidSlash),
                Button = FindViewById<Button>(Resource.Id.btnRapidSlash),
                Name = "連打切り",
                MinDamage = 20,
                MaxDamage = 35,
                NormalColor = Color.ParseColor("#e74c3c"),
                ReadyColor = Color.ParseColor("#ff6b6b")
            });

            // 速攻切り（中ダメージ・青系）
            _skills.Add(new SkillInfo
            {
                Gauge = FindViewById<CircularGaugeView>(Resource.Id.gaugeQuickSlash),
                Button = FindViewById<Button>(Resource.Id.btnQuickSlash),
                Name = "速攻切り",
                MinDamage = 15,
                MaxDamage = 25,
                NormalColor = Color.ParseColor("#3498db"),
                ReadyColor = Color.ParseColor("#5dade2")
            });

            // 防御（回復・緑系）
            _skills.Add(new SkillInfo
            {
                Gauge = FindViewById<CircularGaugeView>(Resource.Id.gaugeDefense),
                Button = FindViewById<Button>(Resource.Id.btnDefense),
                Name = "防御",
                MinDamage = 0,
                MaxDamage = 0,
                NormalColor = Color.ParseColor("#27ae60"),
                ReadyColor = Color.ParseColor("#2ecc71")
            });

            // 各スキルの初期設定とイベント登録
            foreach (var skill in _skills)
            {
                skill.Gauge!.SetProgressColor(skill.NormalColor);
                skill.Button!.Click += (s, e) => OnSkillButtonClick(skill);
            }
        }

        private void StartGaugeTimer()
        {
            _gaugeTimer = new System.Timers.Timer(TimerInterval);
            _gaugeTimer.Elapsed += OnGaugeTimerElapsed;
            _gaugeTimer.AutoReset = true;
            _gaugeTimer.Start();
        }

        private void OnGaugeTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            // 各スキルのゲージを更新
            foreach (var skill in _skills)
            {
                if (skill.Progress < 1.0f)
                {
                    skill.Progress = Math.Min(skill.Progress + GaugeIncrement, 1.0f);
                }
            }

            // UIスレッドで更新
            RunOnUiThread(UpdateSkillUI);
        }

        private void UpdateSkillUI()
        {
            foreach (var skill in _skills)
            {
                skill.Gauge!.Progress = skill.Progress;

                if (skill.Progress >= 1.0f)
                {
                    skill.Button!.Enabled = true;
                    skill.Gauge.SetProgressColor(skill.ReadyColor);
                }
                else
                {
                    skill.Button!.Enabled = false;
                    skill.Gauge.SetProgressColor(skill.NormalColor);
                }
            }
        }

        private void OnSkillButtonClick(SkillInfo usedSkill)
        {
            if (usedSkill.Progress < 1.0f) return;

            if (usedSkill.Name == "防御")
            {
                // 防御: 次の敵の攻撃を無効化
                _isDefending = true;

                // 防御エフェクト（プレイヤーHP表示が光る）
                _playerHpText!.Animate()!
                    .Alpha(0.5f)
                    .SetDuration(100)
                    .WithEndAction(new Java.Lang.Runnable(() =>
                    {
                        _playerHpText.Animate()!
                            .Alpha(1f)
                            .SetDuration(100)
                            .Start();
                    }))!
                    .Start();
            }
            else
            {
                // 攻撃スキル: 敵にダメージを与える
                int damage = new Random().Next(usedSkill.MinDamage, usedSkill.MaxDamage + 1);
                _enemyHp = Math.Max(0, _enemyHp - damage);
                UpdateEnemyHpBar();

                // 敵へのダメージエフェクト
                _enemyCharacter!.Animate()!
                    .ScaleX(1.3f).ScaleY(1.3f)
                    .SetDuration(100)
                    .WithEndAction(new Java.Lang.Runnable(() =>
                    {
                        _enemyCharacter.Animate()!
                            .ScaleX(1f).ScaleY(1f)
                            .SetDuration(100)
                            .Start();
                    }))!
                    .Start();

                // 勝利判定
                if (_enemyHp <= 0)
                {
                    ShowVictory();
                    return;
                }
            }

            // 使用したスキルのゲージをリセット
            usedSkill.Progress = 0f;
            usedSkill.Gauge!.Progress = 0f;
            usedSkill.Button!.Enabled = false;
            usedSkill.Gauge.SetProgressColor(usedSkill.NormalColor);

            // 他のスキルのゲージを -25%
            foreach (var skill in _skills)
            {
                if (skill != usedSkill)
                {
                    skill.Progress = Math.Max(0f, skill.Progress - PenaltyAmount);
                    skill.Gauge!.Progress = skill.Progress;

                    if (skill.Progress < 1.0f)
                    {
                        skill.Button!.Enabled = false;
                        skill.Gauge.SetProgressColor(skill.NormalColor);
                    }
                }
            }
        }

        private void StartEnemyAttackTimer()
        {
            _enemyAttackTimer = new System.Timers.Timer(EnemyAttackInterval);
            _enemyAttackTimer.Elapsed += OnEnemyAttack;
            _enemyAttackTimer.AutoReset = true;
            _enemyAttackTimer.Start();
        }

        private void OnEnemyAttack(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_enemyHp <= 0 || _playerHp <= 0) return;

            RunOnUiThread(() =>
            {
                if (_isDefending)
                {
                    // 防御成功：ダメージ無効
                    _isDefending = false;

                    // 防御成功エフェクト
                    _enemyCharacter!.Animate()!
                        .TranslationX(20f)
                        .SetDuration(50)
                        .WithEndAction(new Java.Lang.Runnable(() =>
                        {
                            _enemyCharacter.Animate()!
                                .TranslationX(0f)
                                .SetDuration(50)
                                .Start();
                        }))!
                        .Start();
                }
                else
                {
                    // プレイヤーにダメージ
                    int damage = new Random().Next(8, 15);
                    _playerHp = Math.Max(0, _playerHp - damage);
                    UpdatePlayerHpText();

                    // 敵の攻撃エフェクト
                    _enemyCharacter!.Animate()!
                        .TranslationY(50f)
                        .SetDuration(100)
                        .WithEndAction(new Java.Lang.Runnable(() =>
                        {
                            _enemyCharacter.Animate()!
                                .TranslationY(0f)
                                .SetDuration(100)
                                .Start();
                        }))!
                        .Start();

                    // 敗北判定
                    if (_playerHp <= 0)
                    {
                        _playerHpText!.Text = "HP: 0";
                        _playerHpText.SetTextColor(Color.ParseColor("#ff6b6b"));
                        _gaugeTimer?.Stop();
                        _enemyAttackTimer?.Stop();
                        foreach (var skill in _skills)
                        {
                            skill.Button!.Enabled = false;
                        }
                    }
                }
            });
        }

        private void UpdateEnemyHpBar()
        {
            if (_enemyHpBar == null || _enemyHpBarMaxWidth == 0) return;

            float hpRatio = (float)_enemyHp / EnemyMaxHp;
            int newWidth = (int)(_enemyHpBarMaxWidth * hpRatio);

            var layoutParams = _enemyHpBar.LayoutParameters;
            layoutParams!.Width = Math.Max(0, newWidth);
            _enemyHpBar.LayoutParameters = layoutParams;
        }

        private void UpdatePlayerHpText()
        {
            if (_playerHpText == null) return;

            _playerHpText.Text = $"HP: {_playerHp}";

            // HPが低くなると色を変更
            if (_playerHp <= 30)
            {
                _playerHpText.SetTextColor(Color.ParseColor("#ff6b6b")); // 赤
            }
            else if (_playerHp <= 60)
            {
                _playerHpText.SetTextColor(Color.ParseColor("#f39c12")); // オレンジ
            }
            else
            {
                _playerHpText.SetTextColor(Color.ParseColor("#4CAF50")); // 緑
            }
        }

        private void ShowVictory()
        {
            // タイマー停止
            _gaugeTimer?.Stop();
            _enemyAttackTimer?.Stop();

            // スキル無効化
            foreach (var skill in _skills)
            {
                skill.Button!.Enabled = false;
            }

            // 報酬計算（ランダム）
            var random = new Random();
            int expReward = random.Next(80, 150);
            int goldReward = random.Next(30, 80);

            // 報酬テキスト設定
            _expRewardText!.Text = $"EXP: +{expReward}";
            _goldRewardText!.Text = $"GOLD: +{goldReward}";

            // 敵を消すアニメーション
            _enemyContainer!.Animate()!
                .Alpha(0f)
                .ScaleX(0f).ScaleY(0f)
                .SetDuration(300)
                .WithEndAction(new Java.Lang.Runnable(() =>
                {
                    _enemyContainer.Visibility = ViewStates.Gone;

                    // 報酬表示
                    _rewardContainer!.Visibility = ViewStates.Visible;
                    _rewardContainer.Alpha = 0f;
                    _rewardContainer.ScaleX = 0.5f;
                    _rewardContainer.ScaleY = 0.5f;

                    _rewardContainer.Animate()!
                        .Alpha(1f)
                        .ScaleX(1f).ScaleY(1f)
                        .SetDuration(300)
                        .Start();
                }))!
                .Start();
        }

        private void GameOver(bool playerWon)
        {
            _gaugeTimer?.Stop();
            _enemyAttackTimer?.Stop();
            foreach (var skill in _skills)
            {
                skill.Button!.Enabled = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _gaugeTimer?.Stop();
            _gaugeTimer?.Dispose();
            _enemyAttackTimer?.Stop();
            _enemyAttackTimer?.Dispose();
        }
    }
}