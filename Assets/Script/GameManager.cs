using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    #region ステータス設定
    //盤面の大きさ
    private const int SIZE_X = 8;
    private const int SIZE_Y = 8;
    //タイルの大きさ
    private const float TILE_SIZE = 1;
    //盤面の大きさとタイルの大きさに対するタイル生成の基点
    private const float TRANSLATE_X = (SIZE_X * TILE_SIZE - TILE_SIZE) / 2;
    private const float TRANSLATE_Y = (SIZE_Y * TILE_SIZE - TILE_SIZE) / 2;
    //各チームのコマの数
    private const int PIECES_SIZE = 16;
    //タイルの大きさに対するコマの大きさの比率
    private const float PIECES_SCALE = 0.6f;
    //マウス情報
    private RaycastHit2D hit;
    private int selectX = 0;
    private int selectY = 0;
    //UI情報
    [SerializeField, Header("キャンバス")] private Canvas ui;
    [SerializeField, Header("hpバー")] private Slider hpBar_original;
    private Slider hpBar;
    private List<Slider> hpBarList = new List<Slider>();
    private Animator ani = null;
    private List<Animator> aniList = new List<Animator>();
    [SerializeField, Header("プレイヤーA")] private GameObject playerA;
    [SerializeField, Header("プレイヤーB")] private GameObject playerB;
    //ポーンが端に行った時のID記憶
    [SerializeField, Header("ポーン進化パネル")] private GameObject pawnChangePanelA, pawnChangePanelB;
    private int pawnChangeId = -1;
    private int pawnChangeType = -1;
    private int pawnChangePos = -1;
    public void GetButtonId(int num)
    {
        pawnChangeType = num;
    }
    //ゲームモード
    private enum eGameMode
    {
        MULTIPLAYER,
        AI
    }
    private eGameMode gameMode = eGameMode.AI;
    //AIレベル
    private int aiLevel = 1;
    //AI評価値
    private int evaluation = 0;
    private int bestEvaluation = -500;
    private int nowEvaluation = 0;
    private int recieveEvaluation = 0;

    private int bestId = -1;
    private int bestMoveX = -1;
    private int bestMoveY = -1;
    //ウルトポイント
    private int ultPointA = 0;
    private int ultPointB = 0;
    private int ultEvaluation = 0;
    [SerializeField, Header("ウルトバーA")] private Slider ultBarA;
    [SerializeField, Header("ウルト画像A")] private Image ultImgA;
    [SerializeField, Header("ウルトバーB")] private Slider ultBarB;
    [SerializeField, Header("ウルト画像B")] private Image ultImgB;
    #endregion
    #region タイル
    //タイルprefab
    [SerializeField, Header("タイル")] private GameObject tileA_original, tileB_original;
    [SerializeField, Header("選択タイル")] private GameObject tileSelected_original, tileSelected_original2;
    [SerializeField, Header("移動タイル")] private GameObject tileMove_original;

    //タイルの状態
    private enum eState
    {
        NONE, //何もない状態
        A_PAWN,
        A_KNIGHT,
        A_BISHOP,
        A_ROOK,
        A_QUEEN,
        A_KING,
        B_PAWN,
        B_KNIGHT,
        B_BISHOP,
        B_ROOK,
        B_QUEEN,
        B_KING,
    }
    class Tile
    {
        public GameObject tileObject = null; //タイルオブジェクト
        public eState state = eState.NONE; //タイルの状態
        public Vector3 pos = Vector3.zero; //タイルの座標
        public int piecesId = -1; //乗っているコマの識別番号
        public Tile()
        {
            tileObject = null;
            state = eState.NONE;
            pos = Vector3.zero;
            piecesId = -1;
        }
    }
    private Tile[,] tile = new Tile[SIZE_X, SIZE_Y];

    //コマ初期配置
    private eState[,] initState = new eState[SIZE_X, SIZE_Y] {
        {eState.A_ROOK,eState.A_KNIGHT,eState.A_BISHOP,eState.A_KING,eState.A_QUEEN,eState.A_BISHOP,eState.A_KNIGHT,eState.A_ROOK},
        {eState.A_PAWN,eState.A_PAWN,eState.A_PAWN,eState.A_PAWN,eState.A_PAWN,eState.A_PAWN,eState.A_PAWN,eState.A_PAWN},
        {eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE},
        {eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE},
        {eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE},
        {eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE,eState.NONE},
        {eState.B_PAWN,eState.B_PAWN,eState.B_PAWN,eState.B_PAWN,eState.B_PAWN,eState.B_PAWN,eState.B_PAWN,eState.B_PAWN},
        {eState.B_ROOK,eState.B_KNIGHT,eState.B_BISHOP,eState.B_KING,eState.B_QUEEN,eState.B_BISHOP,eState.B_KNIGHT,eState.B_ROOK}
    };
    private eState InitState(int x, int y)
    {
        return initState[x, y];
    }

    private List<GameObject> tile_selected = new List<GameObject>();
    private List<GameObject> tile_move = new List<GameObject>();
    private List<GameObject> tile_ult = new List<GameObject>();
    #endregion
    #region コマ
    //コマ
    [SerializeField, Header("ポーンA")] private GameObject A_pawn_original;
    [SerializeField, Header("ナイトA")] private GameObject A_knight_original;
    [SerializeField, Header("ビショップA")] private GameObject A_bishop_original;
    [SerializeField, Header("ルークA")] private GameObject A_rook_original;
    [SerializeField, Header("クイーンA")] private GameObject A_queen_original;
    [SerializeField, Header("キングA")] private GameObject A_king_original;

    [SerializeField, Header("ポーンB")] private GameObject B_pawn_original;
    [SerializeField, Header("ナイトB")] private GameObject B_knight_original;
    [SerializeField, Header("ビショップB")] private GameObject B_bishop_original;
    [SerializeField, Header("ルークB")] private GameObject B_rook_original;
    [SerializeField, Header("クイーンB")] private GameObject B_queen_original;
    [SerializeField, Header("キングB")] private GameObject B_king_original;

    class Pieces
    {
        public GameObject piecesObject = null; //コマオブジェクト
        public eState type = eState.NONE; //コマの種類
        public Vector3 pos = Vector3.zero; //コマの座標
        public int maxHp = 100;
        public int hp = 100; //HP
        public Pieces()
        {
            piecesObject = null;
            type = eState.NONE;
            pos = Vector3.zero;
            maxHp = 100;
            hp = maxHp;
        }

    }
    private Pieces[] a_pieces = new Pieces[PIECES_SIZE];
    private Pieces[] b_pieces = new Pieces[PIECES_SIZE];
    #endregion
    #region フェーズ制御
    //フェーズ制御
    public enum ePhase
    {
        PLAYER1TURN,
        PLAYER2TURN,
    }
    public ePhase phase = ePhase.PLAYER1TURN;
    private bool selectPiece = false;
    private bool playLock = false;
    public int winner = 0;
    private bool pawnChange = false;
    [SerializeField, Header("勝利パネル")] private GameObject winnerPanel;
    [SerializeField, Header("勝利テキスト")] private Text winnerText;
    private bool isPause = false;
    [SerializeField, Header("ポーズパネル")] private GameObject pausePanel;
    #endregion
    #region SE
    private AudioSource audioSource;
    [SerializeField, Header("攻撃音")] private AudioClip seAttack;
    [SerializeField, Header("回復音")] private AudioClip seHeal;
    [SerializeField, Header("選択音")] private AudioClip seSelect;
    #endregion

    void Start()
    {
        //ステータス取得
        audioSource = GetComponent<AudioSource>();
        //ゲームモード設定
        if (TitleManager.GetIsAi())
        {
            gameMode = eGameMode.AI;
            aiLevel = TitleManager.GetLevel();
        }
        else
        {
            gameMode = eGameMode.MULTIPLAYER;
        }
        //全体のタイルに対する初期設定
        for (int i = 0; i < SIZE_X; i++)
        {
            for (int k = 0; k < SIZE_Y; k++)
            {
                tile[i, k] = new Tile(); //初期化
                tile[i, k].state = InitState(i, k); //初期状態
                tile[i, k].pos = new Vector3(i * TILE_SIZE - TRANSLATE_X, k * TILE_SIZE - TRANSLATE_Y, 0); //座標設定
                //8x8タイルの生成
                if ((i + k) % 2 == 0) { tile[i, k].tileObject = Instantiate(tileA_original); }
                else { tile[i, k].tileObject = Instantiate(tileB_original); }
                tile[i, k].tileObject.transform.position = tile[i, k].pos;
                tile[i, k].tileObject.transform.localScale = new Vector3(TILE_SIZE, TILE_SIZE, 1);
                tile[i, k].tileObject.name = "tile" + i + k;
            }
        }

        //コマの初期化
        for (int i = 0; i < PIECES_SIZE; i++)
        {
            a_pieces[i] = new Pieces();
            b_pieces[i] = new Pieces();
        }
        //コマの初期設定
        int an = 0, bn = 0;
        for (int i = 0; i < SIZE_X; i++)
        {
            for (int k = 0; k < SIZE_Y; k++)
            {
                if (tile[i, k].state != eState.NONE)
                {
                    if (tile[i, k].state <= eState.A_KING)
                    {
                        a_pieces[an].type = tile[i, k].state;
                        a_pieces[an].pos = tile[i, k].pos;
                        tile[i, k].piecesId = an;
                        an++;
                        if (an >= PIECES_SIZE) an = PIECES_SIZE - 1;
                    }
                    else
                    {
                        b_pieces[bn].type = tile[i, k].state;
                        b_pieces[bn].pos = tile[i, k].pos;
                        tile[i, k].piecesId = bn + PIECES_SIZE;
                        bn++;
                        if (bn >= PIECES_SIZE) bn = PIECES_SIZE - 1;
                    }
                }
            }
        }
        //コマデータ入力
        for (int i = 0; i < PIECES_SIZE; i++)
        {
            //Aのコマデータ
            switch (a_pieces[i].type)
            {
                case eState.A_PAWN:
                    a_pieces[i].piecesObject = Instantiate(A_pawn_original);
                    a_pieces[i].maxHp = 75;
                    break;
                case eState.A_KNIGHT:
                    a_pieces[i].piecesObject = Instantiate(A_knight_original);
                    a_pieces[i].maxHp = 90;
                    break;
                case eState.A_BISHOP:
                    a_pieces[i].piecesObject = Instantiate(A_bishop_original);
                    a_pieces[i].maxHp = 50;
                    break;
                case eState.A_ROOK:
                    a_pieces[i].piecesObject = Instantiate(A_rook_original);
                    a_pieces[i].maxHp = 150;
                    break;
                case eState.A_QUEEN:
                    a_pieces[i].piecesObject = Instantiate(A_queen_original);
                    a_pieces[i].maxHp = 100;
                    break;
                case eState.A_KING:
                    a_pieces[i].piecesObject = Instantiate(A_king_original);
                    a_pieces[i].maxHp = 200;
                    break;
                default:
                    a_pieces[i].piecesObject = Instantiate(A_pawn_original);
                    a_pieces[i].maxHp = 100;
                    break;
            }
            a_pieces[i].piecesObject.transform.position = a_pieces[i].pos;
            a_pieces[i].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
            //Bのコマデータ
            switch (b_pieces[i].type)
            {
                case eState.B_PAWN:
                    b_pieces[i].piecesObject = Instantiate(B_pawn_original);
                    b_pieces[i].maxHp = 75;
                    break;
                case eState.B_KNIGHT:
                    b_pieces[i].piecesObject = Instantiate(B_knight_original);
                    b_pieces[i].maxHp = 90;
                    break;
                case eState.B_BISHOP:
                    b_pieces[i].piecesObject = Instantiate(B_bishop_original);
                    b_pieces[i].maxHp = 50;
                    break;
                case eState.B_ROOK:
                    b_pieces[i].piecesObject = Instantiate(B_rook_original);
                    b_pieces[i].maxHp = 150;
                    break;
                case eState.B_QUEEN:
                    b_pieces[i].piecesObject = Instantiate(B_queen_original);
                    b_pieces[i].maxHp = 100;
                    break;
                case eState.B_KING:
                    b_pieces[i].piecesObject = Instantiate(B_king_original);
                    b_pieces[i].maxHp = 200;
                    break;
                default:
                    b_pieces[i].piecesObject = Instantiate(B_pawn_original);
                    b_pieces[i].maxHp = 100;
                    break;
            }
            b_pieces[i].piecesObject.transform.position = b_pieces[i].pos;
            b_pieces[i].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
        }
        for (int i = 0; i < PIECES_SIZE; i++)
        {
            a_pieces[i].hp = a_pieces[i].maxHp;
            b_pieces[i].hp = b_pieces[i].maxHp;
        }
    }

    void Update()
    {
        if (isPause) return;
        UltState();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //マウスカーソルの位置からレイを飛ばす
        hit = Physics2D.Raycast(ray.origin, ray.direction); //レイが当たったオブジェクトを取得
        if (hit.collider) //何かに当たった時
        {
            int x = (int)(hit.collider.gameObject.transform.position.x + TRANSLATE_X); //当たったオブジェクトのxを取得
            int y = (int)(hit.collider.gameObject.transform.position.y + TRANSLATE_Y); //yを取得
            //----------------------デバッグ--------------------------------
            //Debug.Log(hit.collider.gameObject.tag);
            //Debug.Log(x + " , " + y);
            //if(tile[x,y].piecesId != -1 && tile[x,y].piecesId < PIECES_SIZE) Debug.Log(a_pieces[tile[x, y].piecesId].hp + "/" + a_pieces[tile[x, y].piecesId].maxHp);
            //else if(tile[x,y].piecesId >= PIECES_SIZE) Debug.Log(b_pieces[tile[x, y].piecesId - PIECES_SIZE].hp + "/" + b_pieces[tile[x, y].piecesId - PIECES_SIZE].maxHp);
            //Debug.Log(pawnChangeId);
            //--------------------------------------------------------------
            switch (gameMode)
            {
                case eGameMode.MULTIPLAYER:
                    MultiPlayer(x, y);
                    break;
                case eGameMode.AI:
                    AiPlayer(x, y);
                    break;
            }
        }
    }

    #region 基本制御
    private void MultiPlayer(int x, int y)
    {
        if (!playLock && winner == 0)
        {
            if (!selectPiece)
            {
                NotSelect(x, y);
                UltAction();
            }
            else
            {
                IsSelected(x, y);
            }
        }
        else if (pawnChange)
        {
            PawnChange();
        }
        else if (winner != 0)
        {
            StartCoroutine(GameSet(winner));
        }
    }

    private void AiPlayer(int x, int y)
    {
        if (!playLock && winner == 0)
        {
            if (phase == ePhase.PLAYER1TURN)
            {
                if (!selectPiece)
                {
                    NotSelect(x, y);
                    UltAction();
                }
                else
                {
                    IsSelected(x, y);
                }
            }
            else
            {
                if (ultPointB == 10)
                {
                    UltAiEvaluation();
                }
                if(ultEvaluation >= 2)
                {
                    ultPointB = 0;
                    ultEvaluation = 0;
                    StartCoroutine(UltAttackB());
                }
                if (!playLock)
                {
                    AiSearching();
                }
            }
        }
        else if (pawnChange)
        {
            if (phase == ePhase.PLAYER1TURN)
            {
                PawnChange();
            }
            else
            {
                StartCoroutine(WaitChangePawn());
            }
        }
        else if (winner != 0)
        {
            StartCoroutine(GameSet(winner));
        }
    }

    private void NotSelect(int x, int y)
    {
        if (selectX != x || selectY != y)
        {
            DeleteSelected(); //攻撃範囲、HPバーの削除
            if (hpBar != null) Destroy(hpBar.gameObject);
        }
        if (x < 0 || y < 0 || x >= SIZE_X || y >= SIZE_Y) return;
        switch (tile[x, y].state) //攻撃範囲の表示
        {
            case eState.A_PAWN:
                SelectPawnA(x, y);
                break;
            case eState.B_PAWN:
                SelectPawnB(x, y);
                break;
            case eState.A_BISHOP:
                SelectBishopA(x, y);
                break;
            case eState.B_BISHOP:
                SelectBishopB(x, y);
                break;
            case eState.A_KNIGHT:
            case eState.B_KNIGHT:
            case eState.A_ROOK:
            case eState.B_ROOK:
            case eState.A_KING:
            case eState.B_KING:
                SelectCircle(x, y);
                break;
            case eState.A_QUEEN:
            case eState.B_QUEEN:
                SelectCircle2(x, y);
                break;
        }
        if (tile[x, y].state != eState.NONE) //コマが置いてあるマスが選択されたとき
        {
            if (hpBar == null) //HPバーの生成
            {
                hpBar = Instantiate(hpBar_original, ui.transform);
                Vector3 sliderPos = new Vector3(tile[x, y].pos.x, tile[x, y].pos.y + 0.5f * TILE_SIZE, 0);
                hpBar.transform.position = sliderPos;
                if (tile[x, y].piecesId < PIECES_SIZE)
                {
                    hpBar.value = (float)a_pieces[tile[x, y].piecesId].hp / (float)a_pieces[tile[x, y].piecesId].maxHp;
                    if (hpBar.value <= 0.15f) hpBar.value = 0.15f;
                }
                else
                {
                    hpBar.value = (float)b_pieces[tile[x, y].piecesId - PIECES_SIZE].hp / (float)b_pieces[tile[x, y].piecesId - PIECES_SIZE].maxHp;
                    if (hpBar.value <= 0.15f) hpBar.value = 0.15f;
                }
            }
            if (Input.GetMouseButtonDown(0)) //マウスがクリックされたら
            {
                if ((tile[x, y].state <= eState.A_KING && phase == ePhase.PLAYER1TURN) || (tile[x, y].state > eState.A_KING && phase == ePhase.PLAYER2TURN))
                {
                    audioSource.PlayOneShot(seSelect);
                    DeleteSelected();
                    selectPiece = true;
                }
            }
        }
    }

    private void IsSelected(int x, int y)
    {
        switch (tile[selectX, selectY].state) //コマの移動範囲を表示
        {
            case eState.A_PAWN:
                MovePawnA(selectX, selectY);
                break;
            case eState.B_PAWN:
                MovePawnB(selectX, selectY);
                break;
            case eState.A_KNIGHT:
            case eState.B_KNIGHT:
                MoveKnight(selectX, selectY);
                break;
            case eState.A_ROOK:
            case eState.B_ROOK:
                MoveRook(selectX, selectY);
                break;
            case eState.A_BISHOP:
            case eState.B_BISHOP:
                MoveBishop(selectX, selectY);
                break;
            case eState.A_QUEEN:
            case eState.B_QUEEN:
                MoveQueen(selectX, selectY);
                break;
            case eState.A_KING:
            case eState.B_KING:
                MoveKing(selectX, selectY);
                break;
        }
        if (Input.GetMouseButtonDown(0)) //マウスがクリックされたら
        {
            if (hit.collider.gameObject.CompareTag("MoveTile")) //移動範囲を選択したら
            {
                audioSource.PlayOneShot(seSelect);
                StartCoroutine(PiecesMove(x, y));
            }
            else
            {
                DeleteMove();//移動範囲の消去
                selectPiece = false;
            }
        }

    }

    private IEnumerator PiecesMove(int x, int y)
    {
        if (phase == ePhase.PLAYER1TURN) //コマの移動
        {
            a_pieces[tile[selectX, selectY].piecesId].pos = tile[x, y].pos;
            //a_pieces[tile[selectX, selectY].piecesId].piecesObject.transform.position = a_pieces[tile[selectX, selectY].piecesId].pos;
            a_pieces[tile[selectX, selectY].piecesId].piecesObject.transform.DOMove(a_pieces[tile[selectX, selectY].piecesId].pos, 1f).SetEase(Ease.InOutCubic);
        }
        else
        {
            b_pieces[tile[selectX, selectY].piecesId - PIECES_SIZE].pos = tile[x, y].pos;
            //b_pieces[tile[selectX, selectY].piecesId - PIECES_SIZE].piecesObject.transform.position = b_pieces[tile[selectX, selectY].piecesId - PIECES_SIZE].pos;
            b_pieces[tile[selectX, selectY].piecesId - PIECES_SIZE].piecesObject.transform.DOMove(b_pieces[tile[selectX, selectY].piecesId - PIECES_SIZE].pos, 1f).SetEase(Ease.InOutCubic);
        }
        tile[x, y].state = tile[selectX, selectY].state; //タイル状態の更新
        tile[x, y].piecesId = tile[selectX, selectY].piecesId;
        tile[selectX, selectY].state = eState.NONE;
        tile[selectX, selectY].piecesId = -1;
        DeleteMove(); //移動範囲の消去
        if (hpBar != null) Destroy(hpBar.gameObject); //HPバーの削除
        playLock = true;
        selectPiece = false;
        yield return new WaitForSeconds(1.5f); //時間差
        StartCoroutine(Attack());
    }

    private IEnumerator GameSet(int winner)
    {
        yield return new WaitForSeconds(2f); //時間差
        winnerPanel.SetActive(true);
        if(winner == 1)
        {
            winnerText.text = "-モンスター軍団の勝利-";
            winnerText.color = Color.red;
        }
        if (winner == 2)
        {
            winnerText.text = "-勇者パーティの勝利-";
            winnerText.color = Color.blue;
        }
    }

    public void Pause()
    {
        isPause = !isPause;
        if(isPause) pausePanel.SetActive(true);
        else pausePanel.SetActive(false);
    }
    #endregion
    #region コマの攻撃範囲
    private void DeleteSelected()
    {
        if (tile_selected.Count > 0)
        {
            for (int i = 0; i < tile_selected.Count; i++)
            {
                Destroy(tile_selected[i]);
            }
            while (tile_selected.Count > 0)
            {
                tile_selected.RemoveAt(0);
            }
        }
    }

    private void SelectPawnA(int x, int y)
    {
        selectX = x;
        selectY = y;
        if (tile_selected.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                tile_selected.Add(Instantiate(tileSelected_original));
            }
            if (x + 1 < SIZE_X && y + 1 < SIZE_Y) tile_selected[0].transform.position = tile[x + 1, y + 1].pos;
            else Destroy(tile_selected[0]);
            if (x + 1 < SIZE_X) tile_selected[1].transform.position = tile[x + 1, y].pos;
            else Destroy(tile_selected[1]);
            if (x + 1 < SIZE_X && y > 0) tile_selected[2].transform.position = tile[x + 1, y - 1].pos;
            else Destroy(tile_selected[2]);
        }
    }

    private void SelectPawnB(int x, int y)
    {
        selectX = x;
        selectY = y;
        if (tile_selected.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                tile_selected.Add(Instantiate(tileSelected_original));
            }
            if (x > 0 && y + 1 < SIZE_Y) tile_selected[0].transform.position = tile[x - 1, y + 1].pos;
            else Destroy(tile_selected[0]);
            if (x > 0) tile_selected[1].transform.position = tile[x - 1, y].pos;
            else Destroy(tile_selected[1]);
            if (x > 0 && y > 0) tile_selected[2].transform.position = tile[x - 1, y - 1].pos;
            else Destroy(tile_selected[2]);
        }
    }

    private void SelectBishopA(int x, int y)
    {
        selectX = x;
        selectY = y;
        if (tile_selected.Count == 0)
        {
            int range = SIZE_X - (x + 1);
            if (range > 3) range = 3;
            for (int i = 0; i < range; i++)
            {
                tile_selected.Add(Instantiate(tileSelected_original));
                tile_selected[i].transform.position = tile[x + (i + 1), y].pos;
            }
        }
    }

    private void SelectBishopB(int x, int y)
    {
        selectX = x;
        selectY = y;
        if (tile_selected.Count == 0)
        {
            int range = x;
            if (range > 3) range = 3;
            for (int i = 0; i < range; i++)
            {
                tile_selected.Add(Instantiate(tileSelected_original));
                tile_selected[i].transform.position = tile[x - (i + 1), y].pos;
            }
        }
    }

    private void SelectCircle(int x, int y)
    {
        selectX = x;
        selectY = y;
        if (tile_selected.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                tile_selected.Add(Instantiate(tileSelected_original));
            }
            if (x + 1 < SIZE_X && y + 1 < SIZE_Y) tile_selected[0].transform.position = tile[x + 1, y + 1].pos;
            else Destroy(tile_selected[0]);
            if (x + 1 < SIZE_X) tile_selected[1].transform.position = tile[x + 1, y].pos;
            else Destroy(tile_selected[1]);
            if (x + 1 < SIZE_X && y > 0) tile_selected[2].transform.position = tile[x + 1, y - 1].pos;
            else Destroy(tile_selected[2]);
            if (x > 0 && y + 1 < SIZE_Y) tile_selected[3].transform.position = tile[x - 1, y + 1].pos;
            else Destroy(tile_selected[3]);
            if (x > 0) tile_selected[4].transform.position = tile[x - 1, y].pos;
            else Destroy(tile_selected[4]);
            if (x > 0 && y > 0) tile_selected[5].transform.position = tile[x - 1, y - 1].pos;
            else Destroy(tile_selected[5]);
            if (y > 0) tile_selected[6].transform.position = tile[x, y - 1].pos;
            else Destroy(tile_selected[6]);
            if (y + 1 < SIZE_Y) tile_selected[7].transform.position = tile[x, y + 1].pos;
            else Destroy(tile_selected[7]);
        }
    }

    private void SelectCircle2(int x, int y)
    {
        selectX = x;
        selectY = y;
        if (tile_selected.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                tile_selected.Add(Instantiate(tileSelected_original2));
            }
            if (x + 1 < SIZE_X && y + 1 < SIZE_Y) tile_selected[0].transform.position = tile[x + 1, y + 1].pos;
            else Destroy(tile_selected[0]);
            if (x + 1 < SIZE_X) tile_selected[1].transform.position = tile[x + 1, y].pos;
            else Destroy(tile_selected[1]);
            if (x + 1 < SIZE_X && y > 0) tile_selected[2].transform.position = tile[x + 1, y - 1].pos;
            else Destroy(tile_selected[2]);
            if (x > 0 && y + 1 < SIZE_Y) tile_selected[3].transform.position = tile[x - 1, y + 1].pos;
            else Destroy(tile_selected[3]);
            if (x > 0) tile_selected[4].transform.position = tile[x - 1, y].pos;
            else Destroy(tile_selected[4]);
            if (x > 0 && y > 0) tile_selected[5].transform.position = tile[x - 1, y - 1].pos;
            else Destroy(tile_selected[5]);
            if (y > 0) tile_selected[6].transform.position = tile[x, y - 1].pos;
            else Destroy(tile_selected[6]);
            if (y + 1 < SIZE_Y) tile_selected[7].transform.position = tile[x, y + 1].pos;
            else Destroy(tile_selected[7]);
        }
    }
    #endregion
    #region コマの移動範囲
    private void DeleteMove()
    {
        if (tile_move.Count > 0)
        {
            for (int i = 0; i < tile_move.Count; i++)
            {
                Destroy(tile_move[i]);
            }
            while (tile_move.Count > 0)
            {
                tile_move.RemoveAt(0);
            }
        }
    }

    private void MovePawnA(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            if (tile[x + 1, y].state == eState.NONE)
            {
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + 1, y].pos;
                if (x + 2 < SIZE_X)
                {
                    if (tile[x + 2, y].state == eState.NONE)
                    {
                        tile_move.Add(Instantiate(tileMove_original));
                        tile_move[tile_move.Count - 1].transform.position = tile[x + 2, y].pos;
                    }
                }
            }
            if (y + 1 < SIZE_Y)
            {
                if (tile[x + 1, y + 1].state == eState.NONE)
                {
                    tile_move.Add(Instantiate(tileMove_original));
                    tile_move[tile_move.Count - 1].transform.position = tile[x + 1, y + 1].pos;
                }
            }
            if (y - 1 > -1)
            {
                if (tile[x + 1, y - 1].state == eState.NONE)
                {
                    tile_move.Add(Instantiate(tileMove_original));
                    tile_move[tile_move.Count - 1].transform.position = tile[x + 1, y - 1].pos;
                }
            }
        }
    }

    private void MovePawnB(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            if (tile[x - 1, y].state == eState.NONE)
            {
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - 1, y].pos;
                if (x - 2 > -1)
                {
                    if (tile[x - 2, y].state == eState.NONE)
                    {
                        tile_move.Add(Instantiate(tileMove_original));
                        tile_move[tile_move.Count - 1].transform.position = tile[x - 2, y].pos;
                    }
                }
            }
            if (y + 1 < SIZE_Y)
            {
                if (tile[x - 1, y + 1].state == eState.NONE)
                {
                    tile_move.Add(Instantiate(tileMove_original));
                    tile_move[tile_move.Count - 1].transform.position = tile[x - 1, y + 1].pos;
                }
            }
            if (y - 1 > -1)
            {
                if (tile[x - 1, y - 1].state == eState.NONE)
                {
                    tile_move.Add(Instantiate(tileMove_original));
                    tile_move[tile_move.Count - 1].transform.position = tile[x - 1, y - 1].pos;
                }
            }
        }
    }

    private void MoveKnight(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                tile_move.Add(Instantiate(tileMove_original));
            }
            if (x + 2 < SIZE_X && y + 1 < SIZE_Y)
            {
                if (tile[x + 2, y + 1].state == eState.NONE) tile_move[0].transform.position = tile[x + 2, y + 1].pos;
                else Destroy(tile_move[0]);
            }
            else Destroy(tile_move[0]);
            if (x + 2 < SIZE_X && y - 1 > -1)
            {
                if (tile[x + 2, y - 1].state == eState.NONE) tile_move[1].transform.position = tile[x + 2, y - 1].pos;
                else Destroy(tile_move[1]);
            }
            else Destroy(tile_move[1]);
            if (x + 1 < SIZE_X && y - 2 > -1)
            {
                if (tile[x + 1, y - 2].state == eState.NONE) tile_move[2].transform.position = tile[x + 1, y - 2].pos;
                else Destroy(tile_move[2]);
            }
            else Destroy(tile_move[2]);
            if (x - 1 > -1 && y - 2 > -1)
            {
                if (tile[x - 1, y - 2].state == eState.NONE) tile_move[3].transform.position = tile[x - 1, y - 2].pos;
                else Destroy(tile_move[3]);
            }
            else Destroy(tile_move[3]);
            if (x - 2 > -1 && y - 1 > -1)
            {
                if (tile[x - 2, y - 1].state == eState.NONE) tile_move[4].transform.position = tile[x - 2, y - 1].pos;
                else Destroy(tile_move[4]);
            }
            else Destroy(tile_move[4]);
            if (x - 2 > -1 && y + 1 < SIZE_Y)
            {
                if (tile[x - 2, y + 1].state == eState.NONE) tile_move[5].transform.position = tile[x - 2, y + 1].pos;
                else Destroy(tile_move[5]);
            }
            else Destroy(tile_move[5]);
            if (x - 1 > -1 && y + 2 < SIZE_Y)
            {
                if (tile[x - 1, y + 2].state == eState.NONE) tile_move[6].transform.position = tile[x - 1, y + 2].pos;
                else Destroy(tile_move[6]);
            }
            else Destroy(tile_move[6]);
            if (x + 1 < SIZE_X && y + 2 < SIZE_Y)
            {
                if (tile[x + 1, y + 2].state == eState.NONE) tile_move[7].transform.position = tile[x + 1, y + 2].pos;
                else Destroy(tile_move[7]);
            }
            else Destroy(tile_move[7]);
        }
    }

    private void MoveRook(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            int i = 1;
            while (x + i < SIZE_X)
            {
                if (tile[x + i, y].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + i, y].pos;
                i++;
            }
            i = 1;
            while (x - i > -1)
            {
                if (tile[x - i, y].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - i, y].pos;
                i++;
            }
            i = 1;
            while (y + i < SIZE_Y)
            {
                if (tile[x, y + i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x, y + i].pos;
                i++;
            }
            i = 1;
            while (y - i > -1)
            {
                if (tile[x, y - i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x, y - i].pos;
                i++;
            }
        }
    }

    private void MoveBishop(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            int i = 1;
            while (x + i < SIZE_X && y + i < SIZE_Y)
            {
                if (tile[x + i, y + i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + i, y + i].pos;
                i++;
            }
            i = 1;
            while (x + i < SIZE_X && y - i > -1)
            {
                if (tile[x + i, y - i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + i, y - i].pos;
                i++;
            }
            i = 1;
            while (x - i > -1 && y + i < SIZE_Y)
            {
                if (tile[x - i, y + i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - i, y + i].pos;
                i++;
            }
            i = 1;
            while (x - i > -1 && y - i > -1)
            {
                if (tile[x - i, y - i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - i, y - i].pos;
                i++;
            }
        }
    }

    private void MoveQueen(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            int i = 1;
            while (x + i < SIZE_X)
            {
                if (tile[x + i, y].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + i, y].pos;
                i++;
            }
            i = 1;
            while (x - i > -1)
            {
                if (tile[x - i, y].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - i, y].pos;
                i++;
            }
            i = 1;
            while (y + i < SIZE_Y)
            {
                if (tile[x, y + i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x, y + i].pos;
                i++;
            }
            i = 1;
            while (y - i > -1)
            {
                if (tile[x, y - i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x, y - i].pos;
                i++;
            }
            i = 1;
            while (x + i < SIZE_X && y + i < SIZE_Y)
            {
                if (tile[x + i, y + i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + i, y + i].pos;
                i++;
            }
            i = 1;
            while (x + i < SIZE_X && y - i > -1)
            {
                if (tile[x + i, y - i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x + i, y - i].pos;
                i++;
            }
            i = 1;
            while (x - i > -1 && y + i < SIZE_Y)
            {
                if (tile[x - i, y + i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - i, y + i].pos;
                i++;
            }
            i = 1;
            while (x - i > -1 && y - i > -1)
            {
                if (tile[x - i, y - i].state != eState.NONE) break;
                tile_move.Add(Instantiate(tileMove_original));
                tile_move[tile_move.Count - 1].transform.position = tile[x - i, y - i].pos;
                i++;
            }
        }
    }

    private void MoveKing(int x, int y)
    {
        if (tile_move.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                tile_move.Add(Instantiate(tileMove_original));
            }
            if (x + 1 < SIZE_X && y + 1 < SIZE_Y)
            {
                if (tile[x + 1, y + 1].state == eState.NONE) tile_move[0].transform.position = tile[x + 1, y + 1].pos;
                else Destroy(tile_move[0]);
            }
            else Destroy(tile_move[0]);
            if (x + 1 < SIZE_X)
            {
                if (tile[x + 1, y].state == eState.NONE) tile_move[1].transform.position = tile[x + 1, y].pos;
                else Destroy(tile_move[1]);
            }
            else Destroy(tile_move[1]);
            if (x + 1 < SIZE_X && y - 1 > -1)
            {
                if (tile[x + 1, y - 1].state == eState.NONE) tile_move[2].transform.position = tile[x + 1, y - 1].pos;
                else Destroy(tile_move[2]);
            }
            else Destroy(tile_move[2]);
            if (x - 1 > -1 && y + 1 < SIZE_Y)
            {
                if (tile[x - 1, y + 1].state == eState.NONE) tile_move[3].transform.position = tile[x - 1, y + 1].pos;
                else Destroy(tile_move[3]);
            }
            else Destroy(tile_move[3]);
            if (x - 1 > -1)
            {
                if (tile[x - 1, y].state == eState.NONE) tile_move[4].transform.position = tile[x - 1, y].pos;
                else Destroy(tile_move[4]);
            }
            else Destroy(tile_move[4]);
            if (x - 1 > -1 && y - 1 > -1)
            {
                if (tile[x - 1, y - 1].state == eState.NONE) tile_move[5].transform.position = tile[x - 1, y - 1].pos;
                else Destroy(tile_move[5]);
            }
            else Destroy(tile_move[5]);
            if (y + 1 < SIZE_Y)
            {
                if (tile[x, y + 1].state == eState.NONE) tile_move[6].transform.position = tile[x, y + 1].pos;
                else Destroy(tile_move[6]);
            }
            else Destroy(tile_move[6]);
            if (y - 1 > -1)
            {
                if (tile[x, y - 1].state == eState.NONE) tile_move[7].transform.position = tile[x, y - 1].pos;
                else Destroy(tile_move[7]);
            }
            else Destroy(tile_move[7]);
        }
    }
    #endregion
    #region 攻撃制御
    private IEnumerator Attack()
    {
        if (phase == ePhase.PLAYER1TURN)
        {
            for (int i = 0; i < PIECES_SIZE; i++) //各コマの攻撃範囲にコマがいたら攻撃
            {
                switch (a_pieces[i].type)
                {
                    case eState.A_PAWN:
                        AttackPawnA(i);
                        break;
                    case eState.A_BISHOP:
                        AttackBishopA(i);
                        break;
                    case eState.A_KNIGHT:
                    case eState.A_ROOK:
                    case eState.A_KING:
                        AttackCircleA(i);
                        break;
                    case eState.A_QUEEN:
                        HealCircleA(i);
                        break;
                }
                if (hpBarList.Count != 0)
                {
                    ani = a_pieces[i].piecesObject.GetComponent<Animator>();
                    ani.SetBool("Attack", true);
                    if(a_pieces[i].type == eState.A_QUEEN) audioSource.PlayOneShot(seHeal);
                    else audioSource.PlayOneShot(seAttack);
                    if (a_pieces[i].type == eState.A_KING) ultPointA++;
                    yield return new WaitForSeconds(0.7f); //時間差
                    ani.SetBool("Attack", false);
                    ani = null;
                    DeleteAniList();
                    DeleteHpBar(); //hpバーの削除
                }
            }
            pawnChange = PawnChangeSearch();
            if (pawnChange)
            {
                pawnChangePanelA.SetActive(true);
            }
            else
            {
                phase = ePhase.PLAYER2TURN; //ターン移行
                playLock = false;
            }
        }
        else
        {
            for (int i = 0; i < PIECES_SIZE; i++)
            {
                switch (b_pieces[i].type)
                {
                    case eState.B_PAWN:
                        AttackPawnB(i);
                        break;
                    case eState.B_BISHOP:
                        AttackBishopB(i);
                        break;
                    case eState.B_KNIGHT:
                    case eState.B_ROOK:
                    case eState.B_KING:
                        AttackCircleB(i);
                        break;
                    case eState.B_QUEEN:
                        HealCircleB(i);
                        break;
                }
                if (hpBarList.Count != 0)
                {
                    ani = b_pieces[i].piecesObject.GetComponent<Animator>();
                    ani.SetBool("Attack", true);
                    if (b_pieces[i].type == eState.B_QUEEN) audioSource.PlayOneShot(seHeal);
                    else audioSource.PlayOneShot(seAttack);
                    if (b_pieces[i].type == eState.B_KING) ultPointB++;
                    yield return new WaitForSeconds(0.7f); //時間差
                    ani.SetBool("Attack", false);
                    ani = null;
                    DeleteAniList();
                    DeleteHpBar(); //hpバーの削除
                }
            }
            pawnChange = PawnChangeSearch();
            if (pawnChange)
            {
                pawnChangePanelB.SetActive(true);
            }
            else
            {
                phase = ePhase.PLAYER1TURN;
                playLock = false;
            }
        }
    }
    private void AttackPawnA(int id)
    {
        int x = (int)(a_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(a_pieces[id].pos.y + TRANSLATE_Y);
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) Damaged(x + 1, y + 1, 10);
        if (x + 1 < SIZE_X) Damaged(x + 1, y, 10);
        if (x + 1 < SIZE_X && y - 1 > -1) Damaged(x + 1, y - 1, 10);
    }

    private void AttackPawnB(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        if (x - 1 > -1 && y + 1 < SIZE_Y) Damaged(x - 1, y + 1, 10);
        if (x - 1 > -1) Damaged(x - 1, y, 10);
        if (x - 1 > -1 && y - 1 > -1) Damaged(x - 1, y - 1, 10);
    }

    private void AttackBishopA(int id)
    {
        int x = (int)(a_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(a_pieces[id].pos.y + TRANSLATE_Y);
        int range = SIZE_X - (x + 1);
        if (range > 3) range = 3;
        for (int i = 0; i < range; i++)
        {
            Damaged(x + (i + 1), y, 20);
        }
    }

    private void AttackBishopB(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        int range = x;
        if (range > 3) range = 3;
        for (int i = 0; i < range; i++)
        {
            Damaged(x - (i + 1), y, 20);
        }
    }

    private void AttackCircleA(int id)
    {
        int damage = 10;
        switch (a_pieces[id].type)
        {
            case eState.A_KNIGHT:
                damage = 25;
                break;
            case eState.A_ROOK:
                damage = 15;
                break;
            case eState.A_KING:
                damage = 30;
                break;
        }
        int x = (int)(a_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(a_pieces[id].pos.y + TRANSLATE_Y);
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) Damaged(x + 1, y + 1, damage);
        if (x + 1 < SIZE_X) Damaged(x + 1, y, damage);
        if (x + 1 < SIZE_X && y > 0) Damaged(x + 1, y - 1, damage);
        if (x > 0 && y + 1 < SIZE_Y) Damaged(x - 1, y + 1, damage);
        if (x > 0) Damaged(x - 1, y, damage);
        if (x > 0 && y > 0) Damaged(x - 1, y - 1, damage);
        if (y > 0) Damaged(x, y - 1, damage);
        if (y + 1 < SIZE_Y) Damaged(x, y + 1, damage);
    }

    private void AttackCircleB(int id)
    {
        int damage = 10;
        switch (b_pieces[id].type)
        {
            case eState.B_KNIGHT:
                damage = 25;
                break;
            case eState.B_ROOK:
                damage = 15;
                break;
            case eState.B_KING:
                damage = 30;
                break;
        }
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) Damaged(x + 1, y + 1, damage);
        if (x + 1 < SIZE_X) Damaged(x + 1, y, damage);
        if (x + 1 < SIZE_X && y > 0) Damaged(x + 1, y - 1, damage);
        if (x > 0 && y + 1 < SIZE_Y) Damaged(x - 1, y + 1, damage);
        if (x > 0) Damaged(x - 1, y, damage);
        if (x > 0 && y > 0) Damaged(x - 1, y - 1, damage);
        if (y > 0) Damaged(x, y - 1, damage);
        if (y + 1 < SIZE_Y) Damaged(x, y + 1, damage);
    }

    private void HealCircleA(int id)
    {
        int x = (int)(a_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(a_pieces[id].pos.y + TRANSLATE_Y);
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) Healed(x + 1, y + 1, 10);
        if (x + 1 < SIZE_X) Healed(x + 1, y, 10);
        if (x + 1 < SIZE_X && y > 0) Healed(x + 1, y - 1, 10);
        if (x > 0 && y + 1 < SIZE_Y) Healed(x - 1, y + 1, 10);
        if (x > 0) Healed(x - 1, y, 10);
        if (x > 0 && y > 0) Healed(x - 1, y - 1, 10);
        if (y > 0) Healed(x, y - 1, 10);
        if (y + 1 < SIZE_Y) Healed(x, y + 1, 10);
    }

    private void HealCircleB(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) Healed(x + 1, y + 1, 10);
        if (x + 1 < SIZE_X) Healed(x + 1, y, 10);
        if (x + 1 < SIZE_X && y > 0) Healed(x + 1, y - 1, 10);
        if (x > 0 && y + 1 < SIZE_Y) Healed(x - 1, y + 1, 10);
        if (x > 0) Healed(x - 1, y, 10);
        if (x > 0 && y > 0) Healed(x - 1, y - 1, 10);
        if (y > 0) Healed(x, y - 1, 10);
        if (y + 1 < SIZE_Y) Healed(x, y + 1, 10);
    }

    private void Damaged(int x, int y, int damage)
    {
        int id = tile[x, y].piecesId;
        if (phase == ePhase.PLAYER1TURN)
        {
            if (tile[x, y].state > eState.A_KING)
            {
                id = id - PIECES_SIZE;
                b_pieces[id].hp -= damage; //ダメージ計算
                if (b_pieces[id].hp <= 0) b_pieces[id].hp = 0;
                hpBarList.Add(Instantiate(hpBar_original, ui.transform)); //hpバー
                Vector3 sliderPos = new Vector3(b_pieces[id].pos.x, b_pieces[id].pos.y + 0.5f * TILE_SIZE, 0);
                hpBarList[hpBarList.Count - 1].transform.position = sliderPos;
                hpBarList[hpBarList.Count - 1].value = (float)b_pieces[id].hp / (float)b_pieces[id].maxHp;
                if (hpBarList[hpBarList.Count - 1].value <= 0.15f && hpBarList[hpBarList.Count - 1].value > 0) hpBarList[hpBarList.Count - 1].value = 0.15f;
                aniList.Add(b_pieces[id].piecesObject.GetComponent<Animator>()); //被弾アニメーション
                aniList[aniList.Count - 1].SetBool("Damage", true);
                if (b_pieces[id].hp == 0)
                {
                    if (b_pieces[id].type == eState.B_KING) winner = 1;
                    tile[x, y].state = eState.NONE;
                    tile[x, y].piecesId = -1;
                    b_pieces[id].type = eState.NONE;
                    aniList[aniList.Count - 1].SetBool("Damage", false);
                    aniList[aniList.Count - 1] = null;
                    aniList.RemoveAt(aniList.Count - 1);
                    Destroy(b_pieces[id].piecesObject.gameObject);
                    ultPointB++;
                }
            }
        }
        else
        {
            if (tile[x, y].state <= eState.A_KING && tile[x, y].state != eState.NONE)
            {
                a_pieces[id].hp -= damage; //ダメージ計算
                if (a_pieces[id].hp <= 0) a_pieces[id].hp = 0;
                hpBarList.Add(Instantiate(hpBar_original, ui.transform)); //hpバー
                Vector3 sliderPos = new Vector3(a_pieces[id].pos.x, a_pieces[id].pos.y + 0.5f * TILE_SIZE, 0);
                hpBarList[hpBarList.Count - 1].transform.position = sliderPos;
                hpBarList[hpBarList.Count - 1].value = (float)a_pieces[id].hp / (float)a_pieces[id].maxHp;
                if (hpBarList[hpBarList.Count - 1].value <= 0.15f && hpBarList[hpBarList.Count - 1].value > 0) hpBarList[hpBarList.Count - 1].value = 0.15f;
                aniList.Add(a_pieces[id].piecesObject.GetComponent<Animator>()); //被弾アニメーション
                aniList[aniList.Count - 1].SetBool("Damage", true);
                if (a_pieces[id].hp == 0)
                {
                    if (a_pieces[id].type == eState.A_KING) winner = 2;
                    tile[x, y].state = eState.NONE;
                    tile[x, y].piecesId = -1;
                    a_pieces[id].type = eState.NONE;
                    aniList[aniList.Count - 1].SetBool("Damage", false);
                    aniList[aniList.Count - 1] = null;
                    aniList.RemoveAt(aniList.Count - 1);
                    Destroy(a_pieces[id].piecesObject.gameObject);
                    ultPointA++;
                }
            }
        }
    }

    private void Healed(int x, int y, int heal)
    {
        int id = tile[x, y].piecesId;
        if (phase == ePhase.PLAYER2TURN)
        {
            if (tile[x, y].state > eState.A_KING)
            {
                id = id - PIECES_SIZE;
                b_pieces[id].hp += heal;
                if (b_pieces[id].hp >= b_pieces[id].maxHp) b_pieces[id].hp = b_pieces[id].maxHp;
                hpBarList.Add(Instantiate(hpBar_original, ui.transform));
                Vector3 sliderPos = new Vector3(b_pieces[id].pos.x, b_pieces[id].pos.y + 0.5f * TILE_SIZE, 0);
                hpBarList[hpBarList.Count - 1].transform.position = sliderPos;
                hpBarList[hpBarList.Count - 1].value = (float)b_pieces[id].hp / (float)b_pieces[id].maxHp;
            }
        }
        else
        {
            if (tile[x, y].state <= eState.A_KING && tile[x, y].state != eState.NONE)
            {
                a_pieces[id].hp += heal;
                if (a_pieces[id].hp >= a_pieces[id].maxHp) a_pieces[id].hp = a_pieces[id].maxHp;
                hpBarList.Add(Instantiate(hpBar_original, ui.transform));
                Vector3 sliderPos = new Vector3(a_pieces[id].pos.x, a_pieces[id].pos.y + 0.5f * TILE_SIZE, 0);
                hpBarList[hpBarList.Count - 1].transform.position = sliderPos;
                hpBarList[hpBarList.Count - 1].value = (float)a_pieces[id].hp / (float)a_pieces[id].maxHp;
            }
        }
    }

    private void DeleteHpBar()
    {
        if (hpBarList.Count > 0)
        {
            for (int i = 0; i < hpBarList.Count; i++)
            {
                Destroy(hpBarList[i].gameObject);
            }
            while (hpBarList.Count > 0)
            {
                hpBarList.RemoveAt(0);
            }
        }
    }

    private void DeleteAniList()
    {
        for (int i = 0; i < aniList.Count; i++)
        {
            aniList[i].SetBool("Damage", false);
            aniList[i] = null;
        }
        while (aniList.Count > 0)
        {
            aniList.RemoveAt(0);
        }
    }
    #endregion
    #region ポーン進化
    private bool PawnChangeSearch()
    {
        if (phase == ePhase.PLAYER1TURN)
        {
            int i = 0;
            while (i < SIZE_Y)
            {
                if (tile[SIZE_X - 1, i].state == eState.A_PAWN)
                {
                    pawnChangeId = tile[SIZE_X - 1, i].piecesId;
                    pawnChangePos = i;
                    return true;
                }
                i++;
            }
            return false;
        }
        else
        {
            int i = 0;
            while (i < SIZE_Y)
            {
                if (tile[0, i].state == eState.B_PAWN)
                {
                    pawnChangeId = tile[0, i].piecesId - PIECES_SIZE;
                    pawnChangePos = i;
                    return true;
                }
                i++;
            }
            return false;
        }
    }

    private void PawnChange()
    {
        if (phase == ePhase.PLAYER1TURN)
        {
            switch (pawnChangeType)
            {
                case 0:
                    a_pieces[pawnChangeId].type = eState.A_KNIGHT;
                    Destroy(a_pieces[pawnChangeId].piecesObject.gameObject);
                    a_pieces[pawnChangeId].piecesObject = Instantiate(A_knight_original);
                    tile[SIZE_X - 1, pawnChangePos].state = eState.A_KNIGHT;
                    a_pieces[pawnChangeId].piecesObject.transform.position = a_pieces[pawnChangeId].pos;
                    a_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    a_pieces[pawnChangeId].maxHp = 90;
                    a_pieces[pawnChangeId].hp = a_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelA.SetActive(false);
                    phase = ePhase.PLAYER2TURN;
                    playLock = false;
                    break;
                case 1:
                    a_pieces[pawnChangeId].type = eState.A_BISHOP;
                    Destroy(a_pieces[pawnChangeId].piecesObject.gameObject);
                    a_pieces[pawnChangeId].piecesObject = Instantiate(A_bishop_original);
                    tile[SIZE_X - 1, pawnChangePos].state = eState.A_BISHOP;
                    a_pieces[pawnChangeId].piecesObject.transform.position = a_pieces[pawnChangeId].pos;
                    a_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    a_pieces[pawnChangeId].maxHp = 50;
                    a_pieces[pawnChangeId].hp = a_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelA.SetActive(false);
                    phase = ePhase.PLAYER2TURN;
                    playLock = false;
                    break;
                case 2:
                    a_pieces[pawnChangeId].type = eState.A_ROOK;
                    Destroy(a_pieces[pawnChangeId].piecesObject.gameObject);
                    a_pieces[pawnChangeId].piecesObject = Instantiate(A_rook_original);
                    tile[SIZE_X - 1, pawnChangePos].state = eState.A_ROOK;
                    a_pieces[pawnChangeId].piecesObject.transform.position = a_pieces[pawnChangeId].pos;
                    a_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    a_pieces[pawnChangeId].maxHp = 150;
                    a_pieces[pawnChangeId].hp = a_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelA.SetActive(false);
                    phase = ePhase.PLAYER2TURN;
                    playLock = false;
                    break;
                case 3:
                    a_pieces[pawnChangeId].type = eState.A_QUEEN;
                    Destroy(a_pieces[pawnChangeId].piecesObject.gameObject);
                    a_pieces[pawnChangeId].piecesObject = Instantiate(A_queen_original);
                    tile[SIZE_X - 1, pawnChangePos].state = eState.A_QUEEN;
                    a_pieces[pawnChangeId].piecesObject.transform.position = a_pieces[pawnChangeId].pos;
                    a_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    a_pieces[pawnChangeId].maxHp = 100;
                    a_pieces[pawnChangeId].hp = a_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelA.SetActive(false);
                    phase = ePhase.PLAYER2TURN;
                    playLock = false;
                    break;
            }
        }
        else
        {
            switch (pawnChangeType)
            {
                case 0:
                    b_pieces[pawnChangeId].type = eState.B_KNIGHT;
                    Destroy(b_pieces[pawnChangeId].piecesObject.gameObject);
                    b_pieces[pawnChangeId].piecesObject = Instantiate(B_knight_original);
                    tile[0, pawnChangePos].state = eState.B_KNIGHT;
                    b_pieces[pawnChangeId].piecesObject.transform.position = b_pieces[pawnChangeId].pos;
                    b_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    b_pieces[pawnChangeId].maxHp = 90;
                    b_pieces[pawnChangeId].hp = b_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelB.SetActive(false);
                    phase = ePhase.PLAYER1TURN;
                    playLock = false;
                    break;
                case 1:
                    b_pieces[pawnChangeId].type = eState.B_BISHOP;
                    Destroy(b_pieces[pawnChangeId].piecesObject.gameObject);
                    b_pieces[pawnChangeId].piecesObject = Instantiate(B_bishop_original);
                    tile[0, pawnChangePos].state = eState.B_BISHOP;
                    b_pieces[pawnChangeId].piecesObject.transform.position = b_pieces[pawnChangeId].pos;
                    b_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    b_pieces[pawnChangeId].maxHp = 50;
                    b_pieces[pawnChangeId].hp = b_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelB.SetActive(false);
                    phase = ePhase.PLAYER1TURN;
                    playLock = false;
                    break;
                case 2:
                    b_pieces[pawnChangeId].type = eState.B_ROOK;
                    Destroy(b_pieces[pawnChangeId].piecesObject.gameObject);
                    b_pieces[pawnChangeId].piecesObject = Instantiate(B_rook_original);
                    tile[0, pawnChangePos].state = eState.B_ROOK;
                    b_pieces[pawnChangeId].piecesObject.transform.position = b_pieces[pawnChangeId].pos;
                    b_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    b_pieces[pawnChangeId].maxHp = 150;
                    b_pieces[pawnChangeId].hp = b_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelB.SetActive(false);
                    phase = ePhase.PLAYER1TURN;
                    playLock = false;
                    break;
                case 3:
                    b_pieces[pawnChangeId].type = eState.B_QUEEN;
                    Destroy(b_pieces[pawnChangeId].piecesObject.gameObject);
                    b_pieces[pawnChangeId].piecesObject = Instantiate(B_queen_original);
                    tile[0, pawnChangePos].state = eState.B_QUEEN;
                    b_pieces[pawnChangeId].piecesObject.transform.position = b_pieces[pawnChangeId].pos;
                    b_pieces[pawnChangeId].piecesObject.transform.localScale = new Vector3(TILE_SIZE * PIECES_SCALE, TILE_SIZE * PIECES_SCALE, 1);
                    b_pieces[pawnChangeId].maxHp = 100;
                    b_pieces[pawnChangeId].hp = b_pieces[pawnChangeId].maxHp;
                    pawnChangeId = -1;
                    pawnChangeType = -1;
                    pawnChange = false;
                    pawnChangePanelB.SetActive(false);
                    phase = ePhase.PLAYER1TURN;
                    playLock = false;
                    break;
            }
        }
    }
    #endregion
    #region AI
    private void AiSearching()
    {
        for(int i = 0; i < PIECES_SIZE; i++)
        {
            switch (b_pieces[i].type)
            {
                case eState.B_PAWN:
                    AiSearchPawn(i);
                    break;
                case eState.B_BISHOP:
                    AiSearchBishop(i);
                    break;
                case eState.B_KNIGHT:
                    AiSearchKnight(i);
                    break;
                case eState.B_ROOK:
                    AiSearchRook(i);
                    break;
                case eState.B_KING:
                    AiSearchKing(i);
                    break;
                case eState.B_QUEEN:
                    AiSearchQueen(i);
                    break;
            }
        }
        selectX = (int)(b_pieces[bestId].pos.x + TRANSLATE_X);
        selectY = (int)(b_pieces[bestId].pos.y + TRANSLATE_Y);
        StartCoroutine(PiecesMove(bestMoveX, bestMoveY));
        evaluation = 0;
        nowEvaluation = 0;
        bestEvaluation = -500;
        recieveEvaluation = 0;
        bestId = -1;
        bestMoveX = -1;
        bestMoveY = -1;
    }

    private void AiSearchPawn(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        AiAttackPawn(x, y);
        if(aiLevel == 1) EvaluationRecieveDamage(id,x, y);
        NowEvaluation();
        if (tile[x - 1, y].state == eState.NONE)
        {
            AiAttackPawn(x - 1, y);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y);
            CompareEvaluation(id, x - 1, y);
            if (x - 2 > -1)
            {
                if (tile[x - 2, y].state == eState.NONE)
                {
                    AiAttackPawn(x - 2, y);
                    if (aiLevel == 1) EvaluationRecieveDamage(id,x - 2, y);
                    CompareEvaluation(id, x - 2, y);
                }
            }
        }
        if (y + 1 < SIZE_Y)
        {
            if (tile[x - 1, y + 1].state == eState.NONE)
            {
                AiAttackPawn(x - 1, y + 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y + 1);
                CompareEvaluation(id, x - 1, y + 1);
            }
        }
        if (y - 1 > -1)
        {
            if (tile[x - 1, y - 1].state == eState.NONE)
            {
                AiAttackPawn(x - 1, y - 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y - 1);
                CompareEvaluation(id, x - 1, y - 1);
            }
        }
        nowEvaluation = 0;
    }

    private void AiAttackPawn(int x,int y)
    {
        int damage = 10;
        if (x - 1 > -1 && y + 1 < SIZE_Y) evaluation += EvaluationDamaged(x - 1, y + 1, damage);
        if (x - 1 > -1) evaluation += EvaluationDamaged(x - 1, y, damage);
        if (x - 1 > -1 && y - 1 > -1) evaluation += EvaluationDamaged(x - 1, y - 1, damage);
        if (x == 0) evaluation += 100;
    }

    private void AiSearchBishop(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        int i = 1;
        AiAttackBishop(x, y);
        if (aiLevel == 1) EvaluationRecieveDamage(id,x, y);
        NowEvaluation();
        while (x + i < SIZE_X && y + i < SIZE_Y)
        {
            if (tile[x + i, y + i].state != eState.NONE) break;
            AiAttackBishop(x + i, y + i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x + i, y + i);
            CompareEvaluation(id, x + i, y + i);
            i++;
        }
        i = 1;
        while (x + i < SIZE_X && y - i > -1)
        {
            if (tile[x + i, y - i].state != eState.NONE) break;
            AiAttackBishop(x + i, y - i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x + i, y - i);
            CompareEvaluation(id, x + i, y - i);
            i++;
        }
        i = 1;
        while (x - i > -1 && y + i < SIZE_Y)
        {
            if (tile[x - i, y + i].state != eState.NONE) break;
            AiAttackBishop(x - i, y + i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - i, y + i);
            CompareEvaluation(id, x - i, y + i);
            i++;
        }
        i = 1;
        while (x - i > -1 && y - i > -1)
        {
            if (tile[x - i, y - i].state != eState.NONE) break;
            AiAttackBishop(x - i, y - i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - i, y - i);
            CompareEvaluation(id, x - i, y - i);
            i++;
        }
        nowEvaluation = 0;
    }

    private void AiAttackBishop(int x, int y)
    {
        int damage = 20;
        int range = x;
        if (range > 3) range = 3;
        for (int i = 0; i < range; i++)
        {
            evaluation += EvaluationDamaged(x - (i + 1), y, damage);
        }
    }

    private void AiSearchKnight(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        AiAttackCircle(id, x, y);
        if (aiLevel == 1) EvaluationRecieveDamage(id,x, y);
        NowEvaluation();
        if (x + 2 < SIZE_X && y + 1 < SIZE_Y)
        {
            if (tile[x + 2, y + 1].state == eState.NONE)
            {
                AiAttackCircle(id, x + 2, y + 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 2, y + 1);
                CompareEvaluation(id, x + 2, y + 1);
            }
        }
        if (x + 2 < SIZE_X && y - 1 > -1)
        {
            if (tile[x + 2, y - 1].state == eState.NONE)
            {
                AiAttackCircle(id, x + 2, y - 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 2, y - 1);
                CompareEvaluation(id, x + 2, y - 1);
            }
        }
        if (x + 1 < SIZE_X && y - 2 > -1)
        {
            if (tile[x + 1, y - 2].state == eState.NONE)
            {
                AiAttackCircle(id, x + 1, y - 2);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 1, y - 2);
                CompareEvaluation(id, x + 1, y - 2);
            }
        }
        if (x - 1 > -1 && y - 2 > -1)
        {
            if (tile[x - 1, y - 2].state == eState.NONE)
            {
                AiAttackCircle(id, x - 1, y - 2);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y - 2);
                CompareEvaluation(id, x - 1, y - 2);
            }
        }
        if (x - 2 > -1 && y - 1 > -1)
        {
            if (tile[x - 2, y - 1].state == eState.NONE)
            {
                AiAttackCircle(id, x - 2, y - 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 2, y - 1);
                CompareEvaluation(id, x - 2, y - 1);
            }
        }
        if (x - 2 > -1 && y + 1 < SIZE_Y)
        {
            if (tile[x - 2, y + 1].state == eState.NONE)
            {
                AiAttackCircle(id, x - 2, y + 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 2, y + 1);
                CompareEvaluation(id, x - 2, y + 1);
            }
        }
        if (x - 1 > -1 && y + 2 < SIZE_Y)
        {
            if (tile[x - 1, y + 2].state == eState.NONE)
            {
                AiAttackCircle(id, x - 1, y + 2);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y + 2);
                CompareEvaluation(id, x - 1, y + 2);
            }
        }
        if (x + 1 < SIZE_X && y + 2 < SIZE_Y)
        {
            if (tile[x + 1, y + 2].state == eState.NONE)
            {
                AiAttackCircle(id, x + 1, y + 2);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 1, y + 2);
                CompareEvaluation(id, x + 1, y + 2);
            }
        }
        nowEvaluation = 0;
    }

    private void AiSearchRook(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        int i = 1;
        AiAttackCircle(id, x, y);
        if (aiLevel == 1) EvaluationRecieveDamage(id,x, y);
        NowEvaluation();
        while (x + i < SIZE_X)
        {
            if (tile[x + i, y].state != eState.NONE) break;
            AiAttackCircle(id, x + i, y);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x + i, y);
            CompareEvaluation(id, x + i, y);
            i++;
        }
        i = 1;
        while (x - i > -1)
        {
            if (tile[x - i, y].state != eState.NONE) break;
            AiAttackCircle(id, x - i, y);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - i, y);
            CompareEvaluation(id, x - i, y);
            i++;
        }
        i = 1;
        while (y + i < SIZE_Y)
        {
            if (tile[x, y + i].state != eState.NONE) break;
            AiAttackCircle(id, x, y + i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x, y + i);
            CompareEvaluation(id, x, y + i);
            i++;
        }
        i = 1;
        while (y - i > -1)
        {
            if (tile[x, y - i].state != eState.NONE) break;
            AiAttackCircle(id, x, y - i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x, y - i);
            CompareEvaluation(id, x, y - i);
            i++;
        }
        nowEvaluation = 0;
    }

    private void AiSearchKing(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        AiAttackCircle(id, x, y);
        if (aiLevel == 1) EvaluationRecieveDamage(id,x, y);
        NowEvaluation();
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y)
        {
            if (tile[x + 1, y + 1].state == eState.NONE)
            {
                AiAttackCircle(id, x + 1, y + 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 1, y + 1);
                CompareEvaluation(id, x + 1, y + 1);
            }
        }
        if (x + 1 < SIZE_X)
        {
            if (tile[x + 1, y].state == eState.NONE)
            {
                AiAttackCircle(id, x + 1, y);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 1, y);
                CompareEvaluation(id, x + 1, y);
            }
        }
        if (x + 1 < SIZE_X && y - 1 > -1)
        {
            if (tile[x + 1, y - 1].state == eState.NONE)
            {
                AiAttackCircle(id, x + 1, y - 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x + 1, y - 1);
                CompareEvaluation(id, x + 1, y - 1);
            }
        }
        if (x - 1 > -1 && y + 1 < SIZE_Y)
        {
            if (tile[x - 1, y + 1].state == eState.NONE)
            {
                AiAttackCircle(id, x - 1, y + 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y + 1);
                CompareEvaluation(id, x - 1, y + 1);
            }
        }
        if (x - 1 > -1)
        {
            if (tile[x - 1, y].state == eState.NONE)
            {
                AiAttackCircle(id, x - 1, y);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y);
                CompareEvaluation(id, x - 1, y);
            }
        }
        if (x - 1 > -1 && y - 1 > -1)
        {
            if (tile[x - 1, y - 1].state == eState.NONE)
            {
                AiAttackCircle(id, x - 1, y - 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x - 1, y - 1);
                CompareEvaluation(id, x - 1, y - 1);
            }
        }
        if (y + 1 < SIZE_Y)
        {
            if (tile[x, y + 1].state == eState.NONE)
            {
                AiAttackCircle(id, x, y + 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x, y + 1);
                CompareEvaluation(id, x, y + 1);
            }
        }
        if (y - 1 > -1)
        {
            if (tile[x, y - 1].state == eState.NONE)
            {
                AiAttackCircle(id, x, y - 1);
                if (aiLevel == 1) EvaluationRecieveDamage(id,x, y - 1);
                CompareEvaluation(id, x, y - 1);
            }
        }
        nowEvaluation = 0;
    }

    private void AiAttackCircle(int id, int x, int y)
    {
        int damage = 10;
        switch (b_pieces[id].type)
        {
            case eState.B_KNIGHT:
                damage = 25;
                break;
            case eState.B_ROOK:
                damage = 15;
                break;
            case eState.B_KING:
                damage = 30;
                break;
        }
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) evaluation += EvaluationDamaged(x + 1, y + 1, damage);
        if (x + 1 < SIZE_X) evaluation += EvaluationDamaged(x + 1, y, damage);
        if (x + 1 < SIZE_X && y > 0) evaluation += EvaluationDamaged(x + 1, y - 1, damage);
        if (x > 0 && y + 1 < SIZE_Y) evaluation += EvaluationDamaged(x - 1, y + 1, damage);
        if (x > 0) evaluation += EvaluationDamaged(x - 1, y, damage);
        if (x > 0 && y > 0) evaluation += EvaluationDamaged(x - 1, y - 1, damage);
        if (y > 0) evaluation += EvaluationDamaged(x, y - 1, damage);
        if (y + 1 < SIZE_Y) evaluation += EvaluationDamaged(x, y + 1, damage);
    }

    private void AiSearchQueen(int id)
    {
        int x = (int)(b_pieces[id].pos.x + TRANSLATE_X);
        int y = (int)(b_pieces[id].pos.y + TRANSLATE_Y);
        AiHealCircle(x, y);
        if (aiLevel == 1) EvaluationRecieveDamage(id,x, y);
        NowEvaluation();
        int i = 1;
        while (x + i < SIZE_X)
        {
            if (tile[x + i, y].state != eState.NONE) break;
            AiHealCircle(x + i, y);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x + i, y);
            CompareEvaluation(id, x + i, y);
            i++;
        }
        i = 1;
        while (x - i > -1)
        {
            if (tile[x - i, y].state != eState.NONE) break;
            AiHealCircle(x - i, y);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - i, y);
            CompareEvaluation(id, x - i, y);
            i++;
        }
        i = 1;
        while (y + i < SIZE_Y)
        {
            if (tile[x, y + i].state != eState.NONE) break;
            AiHealCircle(x, y + i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x, y + i);
            CompareEvaluation(id, x, y + i);
            i++;
        }
        i = 1;
        while (y - i > -1)
        {
            if (tile[x, y - i].state != eState.NONE) break;
            AiHealCircle(x, y - i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x, y - i);
            CompareEvaluation(id, x, y - i);
            i++;
        }
        i = 1;
        while (x + i < SIZE_X && y + i < SIZE_Y)
        {
            if (tile[x + i, y + i].state != eState.NONE) break;
            AiHealCircle(x + i, y + i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x + i, y + i);
            CompareEvaluation(id, x + i, y + i);
            i++;
        }
        i = 1;
        while (x + i < SIZE_X && y - i > -1)
        {
            if (tile[x + i, y - i].state != eState.NONE) break;
            AiHealCircle(x + i, y - i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x + i, y - i);
            CompareEvaluation(id, x + i, y - i);
            i++;
        }
        i = 1;
        while (x - i > -1 && y + i < SIZE_Y)
        {
            if (tile[x - i, y + i].state != eState.NONE) break;
            AiHealCircle(x - i, y + i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - i, y + i);
            CompareEvaluation(id, x - i, y + i);
            i++;
        }
        i = 1;
        while (x - i > -1 && y - i > -1)
        {
            if (tile[x - i, y - i].state != eState.NONE) break;
            AiHealCircle(x - i, y - i);
            if (aiLevel == 1) EvaluationRecieveDamage(id,x - i, y - i);
            CompareEvaluation(id, x - i, y - i);
            i++;
        }
        nowEvaluation = 0;
    }

    private void AiHealCircle(int x, int y)
    {
        int heal = 20;
        if (x + 1 < SIZE_X && y + 1 < SIZE_Y) evaluation += EvaluationHealed(x + 1, y + 1, heal);
        if (x + 1 < SIZE_X) evaluation += EvaluationHealed(x + 1, y, heal);
        if (x + 1 < SIZE_X && y > 0) evaluation += EvaluationHealed(x + 1, y - 1, heal);
        if (x > 0 && y + 1 < SIZE_Y) evaluation += EvaluationHealed(x - 1, y + 1, heal);
        if (x > 0) evaluation += EvaluationHealed(x - 1, y, heal);
        if (x > 0 && y > 0) evaluation += EvaluationHealed(x - 1, y - 1, heal);
        if (y > 0) evaluation += EvaluationHealed(x, y - 1, heal);
        if (y + 1 < SIZE_Y) evaluation += EvaluationHealed(x, y + 1, heal);
    }

    private int EvaluationDamaged(int x, int y, int damage)
    {
        int id = tile[x, y].piecesId;
        if (tile[x, y].state <= eState.A_KING && tile[x, y].state != eState.NONE)
        {
            if (a_pieces[id].hp - damage <= 0)
            {
                if (a_pieces[id].type == eState.A_KING)
                {
                    return 1000;
                }
                else
                {
                    return damage * 4;
                }

            }
            else
            {
                if (a_pieces[id].type == eState.A_KING)
                {
                    return damage * 2;
                }
                else if (a_pieces[id].type == eState.A_QUEEN)
                {
                    return damage * 3;
                }
                else
                {
                    return damage;
                }
            }
        }
        else
        {
            return 0;
        }
    }

    private int EvaluationHealed(int x, int y, int heal)
    {
        int id = tile[x, y].piecesId;
        if (tile[x, y].state > eState.A_KING)
        {
            id = id - PIECES_SIZE;
            if (b_pieces[id].hp + heal >= b_pieces[id].maxHp)
            {
                return b_pieces[id].maxHp - b_pieces[id].hp;
            }
            else
            {
                return heal;
            }
        }
        else
        {
            return 0;
        }
    }

    private void EvaluationRecieveDamage(int id, int x, int y)
    {
        if(x + 1 < SIZE_X)
        {
            if(y + 1 < SIZE_Y)
            {
                switch(tile[x + 1, y + 1].state)
                {
                    case eState.A_KNIGHT:
                        recieveEvaluation += 25;
                        break;
                    case eState.A_ROOK:
                        recieveEvaluation += 15;
                        break;
                    case eState.A_KING:
                        recieveEvaluation += 30;
                        break;
                }
            }
            if(y - 1 > -1)
            {
                switch (tile[x + 1, y - 1].state)
                {
                    case eState.A_KNIGHT:
                        recieveEvaluation += 25;
                        break;
                    case eState.A_ROOK:
                        recieveEvaluation += 15;
                        break;
                    case eState.A_KING:
                        recieveEvaluation += 30;
                        break;
                }
            }
            switch (tile[x + 1, y].state)
            {
                case eState.A_KNIGHT:
                    recieveEvaluation += 25;
                    break;
                case eState.A_ROOK:
                    recieveEvaluation += 15;
                    break;
                case eState.A_KING:
                    recieveEvaluation += 30;
                    break;
            }
        }
        if(x - 1 > -1)
        {
            if (y + 1 < SIZE_Y)
            {
                switch (tile[x - 1, y + 1].state)
                {
                    case eState.A_PAWN:
                        recieveEvaluation += 10;
                        break;
                    case eState.A_KNIGHT:
                        recieveEvaluation += 25;
                        break;
                    case eState.A_ROOK:
                        recieveEvaluation += 15;
                        break;
                    case eState.A_KING:
                        recieveEvaluation += 30;
                        break;
                }
            }
            if (y - 1 > -1)
            {
                switch (tile[x - 1, y - 1].state)
                {
                    case eState.A_PAWN:
                        recieveEvaluation += 10;
                        break;
                    case eState.A_KNIGHT:
                        recieveEvaluation += 25;
                        break;
                    case eState.A_ROOK:
                        recieveEvaluation += 15;
                        break;
                    case eState.A_KING:
                        recieveEvaluation += 30;
                        break;
                }
            }
            switch (tile[x - 1, y].state)
            {
                case eState.A_PAWN:
                    recieveEvaluation += 10;
                    break;
                case eState.A_KNIGHT:
                    recieveEvaluation += 25;
                    break;
                case eState.A_ROOK:
                    recieveEvaluation += 15;
                    break;
                case eState.A_KING:
                    recieveEvaluation += 30;
                    break;
            }
        }
        if(y + 1 < SIZE_Y)
        {
            switch (tile[x, y + 1].state)
            {
                case eState.A_KNIGHT:
                    recieveEvaluation += 25;
                    break;
                case eState.A_ROOK:
                    recieveEvaluation += 15;
                    break;
                case eState.A_KING:
                    recieveEvaluation += 30;
                    break;
            }
        }
        if(y - 1 > -1)
        {
            switch (tile[x, y - 1].state)
            {
                case eState.A_KNIGHT:
                    recieveEvaluation += 25;
                    break;
                case eState.A_ROOK:
                    recieveEvaluation += 15;
                    break;
                case eState.A_KING:
                    recieveEvaluation += 30;
                    break;
            }
        }
        int range = x;
        if (range > 3) range = 3;
        for (int i = 0; i < range; i++)
        {
            if (tile[x - (i + 1), y].state == eState.A_BISHOP) recieveEvaluation += 20;
        }
        if(b_pieces[id].type == eState.B_KING) recieveEvaluation /= 2;
        else if (b_pieces[id].type == eState.B_QUEEN) recieveEvaluation /= 4;
        else recieveEvaluation /= 6;

    }

    private void NowEvaluation()
    {
        nowEvaluation = evaluation - recieveEvaluation;
        recieveEvaluation = 0;
        evaluation = 0;
    }

    private void CompareEvaluation(int id, int x, int y)
    {
        evaluation -= nowEvaluation;
        evaluation -= recieveEvaluation;
        if(evaluation > bestEvaluation)
        {
            bestEvaluation = evaluation;
            bestId = id;
            bestMoveX = x;
            bestMoveY = y;
        }
        else if (evaluation == bestEvaluation)
        {
            int r = Random.Range(0, 10);
            if(r % 2 == 0)
            {
                bestEvaluation = evaluation;
                bestId = id;
                bestMoveX = x;
                bestMoveY = y;
            }
        }
        evaluation = 0;
        recieveEvaluation = 0;
    }

    private IEnumerator WaitChangePawn()
    {
        yield return new WaitForSeconds(1f);
        pawnChangeType = 3;
        PawnChange();
    }
    #endregion
    #region ウルト
    private void UltState()
    {
        //A
        if (ultPointA > 10) ultPointA = 10;
        ultBarA.value = (float)ultPointA / 10;
        ultImgA.color = ultPointA == 10 ? new Color32(230, 230, 230, 255) : new Color32(140, 140, 140, 255);
        //B
        if (ultPointB > 10) ultPointB = 10;
        ultBarB.value = (float)ultPointB / 10;
        ultImgB.color = ultPointB == 10 ? new Color32(230, 230, 230, 255) : new Color32(140, 140, 140, 255);
    }
    private void UltAction()
    {
        //A
        if (hit.collider.gameObject.CompareTag("UltA") && ultPointA == 10)
        {
            ultImgA.color = new Color(255, 255, 255, 255);
            UltRangeA();
            playerA.GetComponent<PlayerA>().SetUltAniA();
            if (Input.GetMouseButtonDown(0) && phase == ePhase.PLAYER1TURN)
            {
                ultPointA = 0;
                DeleteUltRange();
                playerA.GetComponent<PlayerA>().SetDefaultAniA();
                StartCoroutine(UltAttackA());
            }
        }
        //B
        if (hit.collider.gameObject.CompareTag("UltB") && ultPointB == 10)
        {
            ultImgB.color = new Color(255, 255, 255, 255);
            UltRangeB();
            playerB.GetComponent<PlayerB>().SetUltAniB();
            if (Input.GetMouseButtonDown(0) && phase == ePhase.PLAYER2TURN)
            {
                ultPointB = 0;
                DeleteUltRange();
                playerB.GetComponent<PlayerB>().SetDefaultAniB();
                StartCoroutine(UltAttackB());
            }
        }
        if (!hit.collider.gameObject.CompareTag("UltA") && !hit.collider.gameObject.CompareTag("UltB"))
        {
            playerA.GetComponent<PlayerA>().SetDefaultAniA();
            playerB.GetComponent<PlayerB>().SetDefaultAniB();
            DeleteUltRange();
        }
    }

    private void UltRangeA()
    {
        for(int i = 0; i < SIZE_X; i++)
        {
            for(int k = 0; k < SIZE_Y; k++)
            {
                if(tile[i,k].state == eState.A_KING)
                {
                    selectX = i;
                    selectY = k;
                }
            }
        }
        if (tile_ult.Count == 0)
        {
            int range = SIZE_X - (selectX + 1);
            for (int i = 0; i < range; i++)
            {
                tile_ult.Add(Instantiate(tileSelected_original));
                tile_ult[i].transform.position = tile[selectX + (i + 1), selectY].pos;
            }
        }
    }

    private void UltRangeB()
    {
        for (int i = 0; i < SIZE_X; i++)
        {
            for (int k = 0; k < SIZE_Y; k++)
            {
                if (tile[i, k].state == eState.B_KING)
                {
                    selectX = i;
                    selectY = k;
                }
            }
        }
        if (tile_ult.Count == 0)
        {
            int range = selectX;
            for (int i = 0; i < range; i++)
            {
                tile_ult.Add(Instantiate(tileSelected_original));
                tile_ult[i].transform.position = tile[selectX - (i + 1), selectY].pos;
            }
        }
    }

    private void DeleteUltRange()
    {
        if (tile_ult.Count > 0)
        {
            for (int i = 0; i < tile_ult.Count; i++)
            {
                Destroy(tile_ult[i]);
            }
            while (tile_ult.Count > 0)
            {
                tile_ult.RemoveAt(0);
            }
        }
    }

    private IEnumerator UltAttackA()
    {
        playLock = true;
        playerA.GetComponent<PlayerA>().SetUltAniA();
        yield return new WaitForSeconds(0.7f); //時間差
        int range = SIZE_X - (selectX + 1);
        for (int i = 0; i < range; i++)
        {
            Damaged(selectX + (i + 1), selectY, 50);
        }
        if (hpBarList.Count != 0)
        {
            ani = a_pieces[tile[selectX,selectY].piecesId].piecesObject.GetComponent<Animator>();
            ani.SetBool("Attack", true);
            audioSource.PlayOneShot(seAttack);
            yield return new WaitForSeconds(0.7f); //時間差
            ani.SetBool("Attack", false);
            ani = null;
            DeleteAniList();
            DeleteHpBar(); //hpバーの削除
        }
        playerA.GetComponent<PlayerA>().SetDefaultAniA();
        playLock = false;
    }

    private IEnumerator UltAttackB()
    {
        playLock = true;
        playerB.GetComponent<PlayerB>().SetUltAniB();
        yield return new WaitForSeconds(0.7f); //時間差
        int range = selectX;
        for (int i = 0; i < range; i++)
        {
            Damaged(selectX - (i + 1), selectY, 50);
        }
        if (hpBarList.Count != 0)
        {
            ani = b_pieces[tile[selectX, selectY].piecesId - PIECES_SIZE].piecesObject.GetComponent<Animator>();
            ani.SetBool("Attack", true);
            audioSource.PlayOneShot(seAttack);
            yield return new WaitForSeconds(0.7f); //時間差
            ani.SetBool("Attack", false);
            ani = null;
            DeleteAniList();
            DeleteHpBar(); //hpバーの削除
        }
        playerB.GetComponent<PlayerB>().SetDefaultAniB();
        playLock = false;
    }

    private void UltAiEvaluation()
    {
        for (int i = 0; i < SIZE_X; i++)
        {
            for (int k = 0; k < SIZE_Y; k++)
            {
                if (tile[i, k].state == eState.B_KING)
                {
                    selectX = i;
                    selectY = k;
                }
            }
        }
        int range = selectX;
        ultEvaluation = 0;
        for (int i = 0; i < range; i++)
        {
            if(tile[selectX - (i + 1), selectY].state <= eState.A_KING && tile[selectX - (i + 1), selectY].state != eState.NONE)
            {
                ultEvaluation++;
                if(a_pieces[tile[selectX - (i + 1), selectY].piecesId].hp <= 50)
                {
                    ultEvaluation++;
                }
            }
        }
    }
    #endregion
}