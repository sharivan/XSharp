using MMX.Geometry;
using MMX.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    /// <summary>
    /// Partição da área de desenho do jogo.
    /// Usada para dispor as entidades de forma a acelerar a busca de uma determinada entidade na tela de acordo com um retângulo de desenho especificado.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade (deve descender da classe Sprite)</typeparam>
    internal class Partition<T> where T : Entity
    {
        /// <summary>
        /// Elemento/Célula de uma partição.
        /// A partição é dividida em uma matriz bidimensional de células onde cada uma delas são retângulos iguais.
        /// Cada célula armazena uma lista de entidades que possuem intersecção não vazia com ela, facilitando assim a busca por entidades que possuem intersecção não vazia com um retângulo dado.
        /// </summary>
        /// <typeparam name="U">Tipo da entidade (deve descender da classe Sprite)</typeparam>
        private class PartitionCell<U> where U : Entity
        {
            Partition<U> partition; // Partição a qual esta célula pertence
            Box box; // Retângulo que delimita a célula
            List<U> values; // Lista de entides que possuem intersecção não vazia com esta célula

            /// <summary>
            /// Cria uma nova célula para a partição
            /// </summary>
            /// <param name="partition">Partição a qual esta célula pertence</param>
            /// <param name="box">Retângulo que delimita esta célula</param>
            public PartitionCell(Partition<U> partition, Box box)
            {
                this.partition = partition;
                this.box = box;

                values = new List<U>();
            }

            /// <summary>
            /// Insere uma nova entidade nesta célula
            /// </summary>
            /// <param name="value">Entidade a ser adicionada</param>
            public void Insert(U value)
            {
                if (!values.Contains(value))
                    values.Add(value);
            }

            /// <summary>
            /// Obtém a lista de entidades desta célula que possui intersecção não vazia com um retângulo dado
            /// </summary>
            /// <param name="box">Retângulo usado para pesquisa</param>
            /// <param name="result">Lista de resultados a ser obtido</param>
            public void Query(Box box, List<U> result, U exclude, List<U> addictionalExclusionList)
            {
                // Verifica a lista de entidades da célula
                foreach (U value in values)
                {
                    if (exclude != null && exclude.Equals(value))
                        continue;

                    if (addictionalExclusionList != null && addictionalExclusionList.Contains(value))
                        continue;

                    Box intersection = value.BoundingBox & box; // Calcula a intersecção do retângulo de desenho da entidade com o retângulo de pesquisa

                    if (intersection.Area > 0 && !result.Contains(value)) // Se a intersecção for não vazia e se a entidade ainda não estiver na lista de resultados
                        result.Add(value); // adiciona esta entidade à lista
                }
            }

            /// <summary>
            /// Atualiza uma entidade com relaçõa a esta célula, se necessário adicionando-a ou removendo-a da célula
            /// </summary>
            /// <param name="value">Entidade a ser atualizada nesta célula</param>
            public void Update(U value)
            {
                Box intersection = value.BoundingBox & box; // Calcula a interecção
                bool intersectionNull = intersection.Area == 0;

                if (!intersectionNull && !values.Contains(value)) // Se a intersecção for não vazia e a célula ainda não contém esta entidade
                    values.Add(value); // então adiciona-a em sua lista de entidades
                else if (intersectionNull && values.Contains(value)) // Senão, se a intesecção for vazia e esta entidade ainda está contida neta célula
                    values.Remove(value); // remove-a da sua lista de entidades
            }

            /// <summary>
            /// Remove uma entidade desta célula
            /// </summary>
            /// <param name="value">Entidade a ser removida</param>
            public void Remove(U value)
            {
                values.Remove(value);
            }

            /// <summary>
            /// Limpa a lista de entidades desta célula
            /// </summary>
            public void Clear()
            {
                values.Clear();
            }

            /// <summary>
            /// Obtém a quantidade de entidades que possuem intersecção não vazia com esta célula
            /// </summary>
            public int Count
            {
                get
                {
                    return values.Count;
                }
            }
        }

        private Box box; // Retângulo que define esta partição
        private int rows; // Número de linhas da subdivisão
        private int cols; // Número de colunas da subdivisão

        private PartitionCell<T>[,] cells; // Matriz da partição
        private FixedSingle cellWidth; // Largura de cada subdivisão
        private FixedSingle cellHeight; // Altura de cada subdivisão

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
        public void Insert(T item)
        {
            Box box = item.BoundingBox;

            // Calcula os mínimos e máximos absolutos do retângulo que delimita esta partição
            Vector origin = this.box.Origin;
            Vector mins = this.box.Mins + origin;
            Vector maxs = this.box.Maxs + origin;

            // Calcula os mínimos e máximos absolutos do retângulo de desenho da entidade a ser adicionada
            Vector origin1 = box.Origin;
            Vector mins1 = box.Mins + origin1;
            Vector maxs1 = box.Maxs + origin1;

            int startCol = (int) ((mins1.X - mins.X) / cellWidth); // Calcula a coluna da primeira célula a qual interceptará a entidade
            if (startCol < 0)
                startCol = 0;

            int startRow = (int) ((mins1.Y - mins.Y) / cellHeight); // Calcula a primeira linha da primeira célula a qual interceptará a entidade
            if (startRow < 0)
                startRow = 0;

            int endCol = (int) ((maxs1.X - mins.X - 1) / cellWidth); // Calcula a coluna da última célula a qual interceptará a entidade
            if (endCol >= cols)
                endCol = cols - 1;

            int endRow = (int) ((maxs1.Y - mins.Y - 1) / cellHeight); // Calcula a linha da última célula a qual intercepetará a entidade
            if (endRow >= rows)
                endRow = rows - 1;

            // Varre todas as possíveis células que podem interceptar a entidade dada
            for (int i = startCol; i <= endCol; i++)
                for (int j = startRow; j <= endRow; j++)
                {
                    Box box1 = new Box(new Vector(mins.X + cellWidth * i, mins.Y + cellHeight * j), Vector.NULL_VECTOR, new Vector(cellWidth, cellHeight));
                    Box intersection = box1 & box; // Calcula a intesecção

                    if (intersection.Area == 0) // Se a intesecção for vazia, não precisa adicionar a entidade a célula
                        continue;

                    if (cells[i, j] == null) // Verifica se a célula já foi criada antes, caso não tenha sido ainda então a cria
                        cells[i, j] = new PartitionCell<T>(this, box1);

                    cells[i, j].Insert(item); // Insere a entidade na célula
                }
        }

        public List<T> Query(Box box)
        {
            return Query(box, null, null);
        }

        public List<T> Query(Box box, T exclude)
        {
            return Query(box, exclude, null);
        }

        public List<T> Query(Box box, List<T> exclusionList)
        {
            return Query(box, null, exclusionList);
        }

        /// <summary>
        /// Realiza uma busca de quais entidades possuem intesecção não vazia com um retângulo dado
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public List<T> Query(Box box, T exclude, List<T> addictionalExclusionList)
        {
            List<T> result = new List<T>();

            // Calcula os máximos e mínimos absulutos do retângulo que delimita esta partição
            Vector origin = this.box.Origin;
            Vector mins = this.box.Mins + origin;
            Vector maxs = this.box.Maxs + origin;

            // Calcula os máximos e mínimos do retângulo de pesquisa
            Vector origin1 = box.Origin;
            Vector mins1 = box.Mins + origin1;
            Vector maxs1 = box.Maxs + origin1;

            int startCol = (int) ((mins1.X - mins.X) / cellWidth); // Calcula a coluna da primeira célula a qual deverá ser consultada

            if (startCol < 0)
                startCol = 0;

            int startRow = (int) ((mins1.Y - mins.Y) / cellHeight); // Calcula a primeira linha da primeira célula a qual deverá ser consultada

            if (startRow < 0)
                startRow = 0;

            int endCol = (int) ((maxs1.X - mins.X - 1) / cellWidth); // Calcula a colna da última célula a qual deverá ser consultada

            if (endCol >= cols)
                endCol = cols - 1;

            int endRow = (int) ((maxs1.Y - mins.Y - 1) / cellHeight); // Calcula a linha da última célula a qual deverá ser consultada

            if (endRow >= rows)
                endRow = rows - 1;

            // Varre todas as possíveis células que poderão ter intersecção não vazia com o retângulo dado
            for (int i = startCol; i <= endCol; i++)
                for (int j = startRow; j <= endRow; j++)
                    if (cells[i, j] != null) // Para cada célula que já foi previamente criada
                        cells[i, j].Query(box, result, exclude, addictionalExclusionList); // consulta quais entidades possuem intersecção não vazia com o retângulo dado

            return result;
        }

        /// <summary>
        /// Atualiza uma entidade nesta partição.
        /// Este método deve ser chamado sempre que a entidade tiver sua posição ou dimensões alteradas.
        /// </summary>
        /// <param name="item">Entidade a ser atualizada dentro da partição</param>
        public void Update(T item)
        {
            Vector delta = item.Origin - item.LastOrigin; // Obtém o vetor de deslocamento da entidade desde o último tick

            if (delta == Vector.NULL_VECTOR) // Se a entidade não se deslocou desde o último tick então não há nada o que se fazer aqui
                return;

            Box box = item.BoundingBox; // Obtém o retângulo de desenho atual da entidade
            Box box0 = box - delta; // Obtém o retângulo de desenho da entidade antes do deslocamento (do tick anterior)

            // Calcula os máximos e mínimos absolutos do retângulo que delimita esta partição
            Vector origin = this.box.Origin;
            Vector mins = this.box.Mins + origin;
            Vector maxs = this.box.Maxs + origin;

            // Calcula os máximos e mínimos absolutos do rêtângulo de desenho anterior da entidade
            Vector origin0 = box0.Origin;
            Vector mins0 = box0.Mins + origin0;
            Vector maxs0 = box0.Maxs + origin0;

            // Calcula os máximos e mínimos absolutos do retângulo de desenho atual da entidade
            Vector origin1 = box.Origin;
            Vector mins1 = box.Mins + origin1;
            Vector maxs1 = box.Maxs + origin1;

            int startCol = (int) ((FixedSingle.Min(mins0.X, mins1.X) - mins.X) / cellWidth); // Calcula a coluna da primeira célula para qual deverá ser verificada

            if (startCol < 0)
                startCol = 0;

            if (startCol >= cols)
                startCol = cols - 1;

            int startRow = (int) ((FixedSingle.Min(mins0.Y, mins1.Y) - mins.Y) / cellHeight); // Calcula a linha da primeira célula para a qual deverá ser verificada

            if (startRow < 0)
                startRow = 0;

            if (startRow >= rows)
                startRow = rows - 1;

            int endCol = (int) ((FixedSingle.Max(maxs0.X, maxs1.X) - mins.X - 1) / cellWidth); // Calcula a coluna da útlima célula para qual deverá ser verificada

            if (endCol < 0)
                endCol = 0;

            if (endCol >= cols)
                endCol = cols - 1;

            int endRow = (int) ((FixedSingle.Max(maxs0.Y, maxs1.Y) - mins.Y - 1) / cellHeight); // Calcula a linha da última célula para qual deverá ser verificada

            if (endRow < 0)
                endRow = 0;

            if (endRow >= rows)
                endRow = rows - 1;

            // Varre todas as possíveis células que possui ou possuiam intersecção não vazia com a entidade dada
            for (int i = startCol; i <= endCol; i++)
                for (int j = startRow; j <= endRow; j++)
                    if (cells[i, j] != null) // Se a célula já existir
                    {
                        cells[i, j].Update(item); // Atualiza a entidade dentro da célula

                        if (cells[i, j].Count == 0) // Se a célula não possuir mais entidades, defina como nula
                            cells[i, j] = null;
                    }
                    else
                    {
                        // Senão...
                        Box box1 = new Box(new Vector(mins.X + cellWidth * i, mins.Y + cellHeight * j), Vector.NULL_VECTOR, new Vector(cellWidth, cellHeight));
                        Box intersection = box1 & box; // Calcula a intersecção desta célula com o retângulo de desenho atual da entidade

                        if (intersection.Area == 0) // Se ela for vazia, não há nada o que fazer nesta célula
                            continue;

                        // Senão...
                        if (cells[i, j] == null) // Verifica se a célula é nula
                            cells[i, j] = new PartitionCell<T>(this, box1); // Se for, cria uma nova célula nesta posição

                        cells[i, j].Insert(item); // e finalmente insere a entidade nesta célula
                    }
        }

        /// <summary>
        /// Remove uma entidade da partição
        /// </summary>
        /// <param name="item">Entidade a ser removida</param>
        public void Remove(T item)
        {
            Box box = item.BoundingBox; // Obtém o retângulo de desenho da entidade

            // Calcula os máximos e mínimos absolutos do retângulo que delimita esta partição
            Vector origin = this.box.Origin;
            Vector mins = this.box.Mins + origin;
            Vector maxs = this.box.Maxs + origin;

            // Calcula os máximos e mínimos absolutos do retângulo de desenho da entidade a ser removida
            Vector origin1 = box.Origin;
            Vector mins1 = box.Mins + origin1;
            Vector maxs1 = box.Maxs + origin1;

            int startCol = (int) ((mins1.X - mins.X) / cellWidth); // Calcula a coluna da primeira célula a ser verificada

            if (startCol < 0)
                startCol = 0;

            if (startCol >= cols)
                startCol = cols - 1;

            int startRow = (int) ((mins1.Y - mins.Y) / cellHeight); // Calcula a linha da primeira célula a ser verificada

            if (startRow < 0)
                startRow = 0;

            if (startRow >= rows)
                startRow = rows - 1;

            int endCol = (int) ((maxs1.X - mins.X - 1) / cellWidth); // Calcula a coluna da última célula a ser verificada

            if (endCol < 0)
                endCol = 0;

            if (endCol >= cols)
                endCol = cols - 1;

            int endRow = (int) ((maxs1.Y - mins.Y - 1) / cellHeight); // Calcula a linha da última célula a ser verificada

            if (endRow < 0)
                endRow = 0;

            if (endRow >= rows)
                endRow = rows - 1;

            // Varre todas as possíveis células que podem ter intersecção não vazia com a entidade dada
            for (int i = startCol; i <= endCol; i++)
                for (int j = startRow; j <= endRow; j++)
                    if (cells[i, j] != null)
                    {
                        cells[i, j].Remove(item); // Remove a entidade da célula caso ela possua intersecção não vazia com a célula

                        if (cells[i, j].Count == 0) // Se a célula não possuir mais entidades
                            cells[i, j] = null; // defina-a como nula
                    }
        }

        /// <summary>
        /// Exclui todas as entidades contidas na partição
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < cols; i++)
                for (int j = 0; j < rows; j++)
                    if (cells[i, j] != null)
                    {
                        cells[i, j].Clear();
                        cells[i, j] = null;
                    }
        }
    }
}
