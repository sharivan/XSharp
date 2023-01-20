using System.Collections.Generic;

using MMX.Geometry;
using MMX.Math;

using MMX.Engine.Entities;

namespace MMX.Engine.World
{
    /// <summary>
    /// Partição da área de desenho do jogo.
    /// Usada para dispor as entidades de forma a acelerar a busca de uma determinada entidade na tela de acordo com um retângulo de desenho especificado.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade (deve descender da classe Sprite)</typeparam>
    internal class Partition<T> where T : Entity
    {
        public const int BOXKIND_COUNT = 2;

        /// <summary>
        /// Elemento/Célula de uma partição.
        /// A partição é dividida em uma matriz bidimensional de células onde cada uma delas são retângulos iguais.
        /// Cada célula armazena uma lista de entidades que possuem intersecção não vazia com ela, facilitando assim a busca por entidades que possuem intersecção não vazia com um retângulo dado.
        /// </summary>
        /// <typeparam name="U">Tipo da entidade (deve descender da classe Sprite)</typeparam>
        private class PartitionCell<U> where U : Entity
        {
            readonly Partition<U> partition; // Partição a qual esta célula pertence
            readonly Box box; // Retângulo que delimita a célula
            readonly List<U>[] values;

            /// <summary>
            /// Cria uma nova célula para a partição
            /// </summary>
            /// <param name="partition">Partição a qual esta célula pertence</param>
            /// <param name="box">Retângulo que delimita esta célula</param>
            public PartitionCell(Partition<U> partition, Box box)
            {
                this.partition = partition;
                this.box = box;

                values = new List<U>[BOXKIND_COUNT];

                for (int i = 0; i < BOXKIND_COUNT; i++)
                    values[i] = new List<U>();
            }

            public void Insert(U value, BoxKind kind)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    if (((1 << i) & (int) kind) == 0)
                        continue;

                    List<U> list = values[i];
                    if (!list.Contains(value))
                        list.Add(value);
                }
            }

            public void Query(Box box, List<U> result, U exclude, List<U> addictionalExclusionList, BoxKind kind)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    if (((1 << i) & (int) kind) == 0)
                        continue;

                    List<U> list = values[i];
                    BoxKind k = (BoxKind) (1 << i);

                    // Verifica a lista de entidades da célula
                    foreach (U value in list)
                    {
                        if (exclude != null && exclude.Equals(value))
                            continue;

                        if (addictionalExclusionList != null && addictionalExclusionList.Contains(value))
                            continue;

                        Box intersection = value.GetBox(k) & box; // Calcula a intersecção do retângulo de desenho da entidade com o retângulo de pesquisa

                        if (intersection.IsValid() && !result.Contains(value)) // Se a intersecção for não vazia e se a entidade ainda não estiver na lista de resultados
                            result.Add(value); // adiciona esta entidade à lista
                    }
                }
            }

            /// <summary>
            /// Atualiza uma entidade com relaçõa a esta célula, se necessário adicionando-a ou removendo-a da célula
            /// </summary>
            /// <param name="value">Entidade a ser atualizada nesta célula</param>
            public void Update(U value, BoxKind kind)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    if (((1 << i) & (int) kind) == 0)
                        continue;

                    List<U> list = values[i];
                    BoxKind k = (BoxKind) (1 << i);

                    Box intersection = value.GetBox(k) & box; // Calcula a interecção
                    bool intersectionNull = !intersection.IsValid();

                    if (!intersectionNull && !list.Contains(value)) // Se a intersecção for não vazia e a célula ainda não contém esta entidade
                        list.Add(value); // então adiciona-a em sua lista de entidades
                    else if (intersectionNull && list.Contains(value)) // Senão, se a intesecção for vazia e esta entidade ainda está contida neta célula
                        list.Remove(value); // remove-a da sua lista de entidades
                }
            }

            /// <summary>
            /// Remove uma entidade desta célula
            /// </summary>
            /// <param name="value">Entidade a ser removida</param>
            public void Remove(U value, BoxKind kind = BoxKind.ALL)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    if (((1 << i) & (int) kind) != 0)
                        values[i].Remove(value);
                }
            }

            /// <summary>
            /// Limpa a lista de entidades desta célula
            /// </summary>
            public void Clear(BoxKind kind)
            {
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    if (((1 << i) & (int) kind) != 0)
                        values[i].Clear();
                }
            }

            /// <summary>
            /// Obtém a quantidade de entidades que possuem intersecção não vazia com esta célula
            /// </summary>
            public int Count(BoxKind kind)
            {
                int result = 0;
                for (int i = 0; i < BOXKIND_COUNT; i++)
                {
                    if (((1 << i) & (int) kind) != 0)
                        result += values[i].Count;
                }

                return result;
            }
        }

        private readonly Box box; // Retângulo que define esta partição
        private readonly int rows; // Número de linhas da subdivisão
        private readonly int cols; // Número de colunas da subdivisão

        private readonly PartitionCell<T>[,] cells; // Matriz da partição
        private readonly FixedSingle cellWidth; // Largura de cada subdivisão
        private readonly FixedSingle cellHeight; // Altura de cada subdivisão

        /// <summary>
        /// Cria uma nova partição
        /// </summary>
        /// <param name="left">Coordenada x do topo superior esquerdo da partição</param>
        /// <param name="top">Coordenada y do topo superior esquerdo da partição</param>
        /// <param name="width">Largura da partição</param>
        /// <param name="height">Altura da partição</param>
        /// <param name="rows">Número de linhas da subdivisão da partição</param>
        /// <param name="cols">Número de colunas da subdivisão da partição</param>
        public Partition(FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height, int rows, int cols)
        : this(new Box(new Vector(left, top), Vector.NULL_VECTOR, new Vector(width, height)), rows, cols)
        {
        }

        /// <summary>
        /// Cria uma nova partição
        /// </summary>
        /// <param name="box">Retângulo que delimita a partição</param>
        /// <param name="rows">Número de linhas da subdivisão da partição</param>
        /// <param name="cols">Número de colunas da subdivisão da partição</param>
        public Partition(Box box, int rows, int cols)
        {
            this.box = box;
            this.rows = rows;
            this.cols = cols;

            cellWidth = box.Width / cols; // Calcula a largura de cada subdivisão
            cellHeight = box.Height / rows; // Calcula a altura de cada subdivisão

            cells = new PartitionCell<T>[cols, rows]; // Cria a matriz de subdivisões
        }

        /// <summary>
        /// Insere uma nova entidade a partição
        /// </summary>
        /// <param name="item">Entidade a ser adicionada</param>
        public void Insert(T item, BoxKind kind = BoxKind.ALL)
        {
            for (int i = 0; i < BOXKIND_COUNT; i++)
            {
                if (((1 << i) & (int) kind) == 0)
                    continue;

                var k = (BoxKind) (1 << i);

                Box box = item.GetBox(k);

                // Calcula os mínimos e máximos absolutos do retângulo que delimita esta partição
                Vector lt = this.box.LeftTop;
                Vector rb = this.box.RightBottom;

                // Calcula os mínimos e máximos absolutos do retângulo de desenho da entidade a ser adicionada
                Vector queryLT = box.LeftTop;
                Vector queryRB = box.RightBottom;

                int startCol = ((queryLT.X - lt.X) / cellWidth).Floor(); // Calcula a coluna da primeira célula a qual interceptará a entidade
                if (startCol < 0)
                    startCol = 0;

                int startRow = ((queryLT.Y - lt.Y) / cellHeight).Floor(); // Calcula a primeira linha da primeira célula a qual interceptará a entidade
                if (startRow < 0)
                    startRow = 0;

                int endCol = ((queryRB.X - lt.X - 1) / cellWidth).Ceil(); // Calcula a coluna da última célula a qual interceptará a entidade
                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = ((queryRB.Y - lt.Y - 1) / cellHeight).Ceil(); // Calcula a linha da última célula a qual intercepetará a entidade
                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que podem interceptar a entidade dada
                for (int col = startCol; col <= endCol; col++)
                    for (int row = startRow; row <= endRow; row++)
                    {
                        var box1 = new Box(new Vector(lt.X + cellWidth * col, lt.Y + cellHeight * row), Vector.NULL_VECTOR, new Vector(cellWidth, cellHeight));
                        Box intersection = box1 & box; // Calcula a intesecção

                        if (!intersection.IsValid()) // Se a intesecção for vazia, não precisa adicionar a entidade a célula
                            continue;

                        if (cells[col, row] == null) // Verifica se a célula já foi criada antes, caso não tenha sido ainda então a cria
                            cells[col, row] = new PartitionCell<T>(this, box1);

                        cells[col, row].Insert(item, k); // Insere a entidade na célula
                    }
            }
        }

        public List<T> Query(Box box, BoxKind kind = BoxKind.ALL) => Query(box, null, null, kind);

        public List<T> Query(Box box, T exclude, BoxKind kind = BoxKind.ALL) => Query(box, exclude, null, kind);

        public List<T> Query(Box box, List<T> exclusionList, BoxKind kind = BoxKind.ALL) => Query(box, null, exclusionList, kind);

        /// <summary>
        /// Realiza uma busca de quais entidades possuem intesecção não vazia com um retângulo dado
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public List<T> Query(Box box, T exclude, List<T> addictionalExclusionList, BoxKind kind = BoxKind.ALL)
        {
            var result = new List<T>();

            // Calcula os máximos e mínimos absulutos do retângulo que delimita esta partição
            Vector lt = this.box.LeftTop;
            Vector rb = this.box.RightBottom;

            // Calcula os máximos e mínimos do retângulo de pesquisa
            Vector queryLT = box.LeftTop;
            Vector queryRB = box.RightBottom;

            int startCol = ((queryLT.X - lt.X) / cellWidth).Floor(); // Calcula a coluna da primeira célula a qual deverá ser consultada

            if (startCol < 0)
                startCol = 0;

            int startRow = ((queryLT.Y - lt.Y) / cellHeight).Floor(); // Calcula a primeira linha da primeira célula a qual deverá ser consultada

            if (startRow < 0)
                startRow = 0;

            int endCol = ((queryRB.X - lt.X - 1) / cellWidth).Ceil(); // Calcula a colna da última célula a qual deverá ser consultada

            if (endCol >= cols)
                endCol = cols - 1;

            int endRow = ((queryRB.Y - lt.Y - 1) / cellHeight).Ceil(); // Calcula a linha da última célula a qual deverá ser consultada

            if (endRow >= rows)
                endRow = rows - 1;

            // Varre todas as possíveis células que poderão ter intersecção não vazia com o retângulo dado
            for (int col = startCol; col <= endCol; col++)
                for (int row = startRow; row <= endRow; row++)
                    cells[col, row]?.Query(box, result, exclude, addictionalExclusionList, kind); // consulta quais entidades possuem intersecção não vazia com o retângulo dado

            return result;
        }

        /// <summary>
        /// Atualiza uma entidade nesta partição.
        /// Este método deve ser chamado sempre que a entidade tiver sua posição ou dimensões alteradas.
        /// </summary>
        /// <param name="item">Entidade a ser atualizada dentro da partição</param>
        public void Update(T item, BoxKind kind = BoxKind.ALL)
        {
            Vector delta = item.Origin - item.LastOrigin; // Obtém o vetor de deslocamento da entidade desde o último tick

            if (delta == Vector.NULL_VECTOR) // Se a entidade não se deslocou desde o último tick então não há nada o que se fazer aqui
                return;

            for (int i = 0; i < BOXKIND_COUNT; i++)
            {
                if (((1 << i) & (int) kind) == 0)
                    continue;

                var k = (BoxKind) (1 << i);

                Box box = item.GetBox(k);
                Box box0 = box - delta;

                // Calcula os máximos e mínimos absolutos do retângulo que delimita esta partição
                Vector lt = this.box.LeftTop;
                Vector rb = this.box.RightBottom;

                // Calcula os máximos e mínimos absolutos do rêtângulo de desenho anterior da entidade
                Vector queryOldLT = box0.LeftTop;
                Vector queryOldRB = box0.RightBottom;

                // Calcula os máximos e mínimos absolutos do retângulo de desenho atual da entidade
                Vector queryNewLT = box.LeftTop;
                Vector queryNewRB = box.RightBottom;

                int startCol = ((FixedSingle.Min(queryOldLT.X, queryNewLT.X) - lt.X) / cellWidth).Floor(); // Calcula a coluna da primeira célula para qual deverá ser verificada

                if (startCol < 0)
                    startCol = 0;

                if (startCol >= cols)
                    startCol = cols - 1;

                int startRow = ((FixedSingle.Min(queryOldLT.Y, queryNewLT.Y) - lt.Y) / cellHeight).Floor(); // Calcula a linha da primeira célula para a qual deverá ser verificada

                if (startRow < 0)
                    startRow = 0;

                if (startRow >= rows)
                    startRow = rows - 1;

                int endCol = ((FixedSingle.Max(queryOldRB.X, queryNewRB.X) - lt.X - 1) / cellWidth).Ceil(); // Calcula a coluna da útlima célula para qual deverá ser verificada

                if (endCol < 0)
                    endCol = 0;

                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = ((FixedSingle.Max(queryOldRB.Y, queryNewRB.Y) - lt.Y - 1) / cellHeight).Ceil(); // Calcula a linha da última célula para qual deverá ser verificada

                if (endRow < 0)
                    endRow = 0;

                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que possui ou possuiam intersecção não vazia com a entidade dada
                for (int col = startCol; col <= endCol; col++)
                    for (int row = startRow; row <= endRow; row++)
                        if (cells[col, row] != null) // Se a célula já existir
                        {
                            cells[col, row].Update(item, kind); // Atualiza a entidade dentro da célula

                            if (cells[col, row].Count(BoxKind.ALL) == 0) // Se a célula não possuir mais entidades, defina como nula
                                cells[col, row] = null;
                        }
                        else
                        {
                            // Senão...
                            var box1 = new Box(new Vector(lt.X + cellWidth * col, lt.Y + cellHeight * row), Vector.NULL_VECTOR, new Vector(cellWidth, cellHeight));
                            Box intersection = box1 & box; // Calcula a intersecção desta célula com o retângulo de desenho atual da entidade

                            if (!intersection.IsValid()) // Se ela for vazia, não há nada o que fazer nesta célula
                                continue;

                            // Senão...
                            if (cells[col, row] == null) // Verifica se a célula é nula
                                cells[col, row] = new PartitionCell<T>(this, box1); // Se for, cria uma nova célula nesta posição

                            cells[col, row].Insert(item, k); // e finalmente insere a entidade nesta célula
                        }
            }
        }

        /// <summary>
        /// Remove uma entidade da partição
        /// </summary>
        /// <param name="item">Entidade a ser removida</param>
        public void Remove(T item, BoxKind kind = BoxKind.ALL)
        {
            for (int i = 0; i < BOXKIND_COUNT; i++)
            {
                if (((1 << i) & (int) kind) == 0)
                    continue;

                var k = (BoxKind) (1 << i);

                Box box = item.GetBox(k);

                // Calcula os máximos e mínimos absolutos do retângulo que delimita esta partição
                Vector lt = this.box.LeftTop;
                Vector rb = this.box.RightBottom;

                // Calcula os máximos e mínimos absolutos do retângulo de desenho da entidade a ser removida
                Vector queryLT = box.LeftTop;
                Vector queryRB = box.RightBottom;

                int startCol = ((queryLT.X - lt.X) / cellWidth).Floor(); // Calcula a coluna da primeira célula a ser verificada

                if (startCol < 0)
                    startCol = 0;

                if (startCol >= cols)
                    startCol = cols - 1;

                int startRow = ((queryLT.Y - lt.Y) / cellHeight).Floor(); // Calcula a linha da primeira célula a ser verificada

                if (startRow < 0)
                    startRow = 0;

                if (startRow >= rows)
                    startRow = rows - 1;

                int endCol = ((queryRB.X - lt.X - 1) / cellWidth).Ceil(); // Calcula a coluna da última célula a ser verificada

                if (endCol < 0)
                    endCol = 0;

                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = ((queryRB.Y - lt.Y - 1) / cellHeight).Ceil(); // Calcula a linha da última célula a ser verificada

                if (endRow < 0)
                    endRow = 0;

                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que podem ter intersecção não vazia com a entidade dada
                for (int col = startCol; col <= endCol; col++)
                    for (int row = startRow; row <= endRow; row++)
                        if (cells[col, row] != null)
                        {
                            cells[col, row].Remove(item, kind); // Remove a entidade da célula caso ela possua intersecção não vazia com a célula

                            if (cells[col, row].Count(BoxKind.ALL) == 0) // Se a célula não possuir mais entidades
                                cells[col, row] = null; // defina-a como nula
                        }
            }
        }

        /// <summary>
        /// Exclui todas as entidades contidas na partição
        /// </summary>
        public void Clear(BoxKind kind = BoxKind.ALL)
        {
            for (int col = 0; col < cols; col++)
                for (int row = 0; row < rows; row++)
                    if (cells[col, row] != null)
                    {
                        cells[col, row].Clear(kind);

                        if (cells[col, row].Count(BoxKind.ALL) == 0) // Se a célula não possuir mais entidades
                            cells[col, row] = null; // defina-a como nula
                    }
        }
    }
}
