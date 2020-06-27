/*
 * 
 * API composta por classes usadas para representação de um grafo bidimensional
 * 
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mega_Man_Xvan
{
    /// <summary>
    /// Coordenadas (linha e coluna) de um nó pertencente a um grafo bidimensional
    /// </summary>
    public class Graph2DCoords
    {
        private int row; // Linha
        private int col; // Coluna

        /// <summary>
        /// Cria uma nova classe contendo as coordenadas de um nó pertencente a um grafo bidimensional
        /// </summary>
        /// <param name="row">Linha</param>
        /// <param name="col">Coluna</param>
        public Graph2DCoords(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        /// <summary>
        /// Linha
        /// </summary>
        public int Row
        {
            get { return row; }
        }

        /// <summary>
        /// Coluna
        /// </summary>
        public int Col
        {
            get { return col; }
        }

        public override int GetHashCode()
        {
            return 256 * row + col;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is Graph2DCoords))
                return false;

            Graph2DCoords coords = (Graph2DCoords)obj;
            return row == coords.row && col == coords.col;
        }
    }

    /// <summary>
    /// Grafo bidimensional.
    /// Tal grafo constitui de nós arranjados de forma bidimensional por linhas como se fosse uma matriz.
    /// Cada nó deste grafo está associado a uma única linha e uma única coluna.
    /// </summary>
    public class Graph2D
    {
        public static readonly int INFINITE = 9999; // Valor usado como o infinito

        private static readonly int LEFT = 0; // Esquerda
        private static readonly int UP = 1; // Cima
        private static readonly int RIGHT = 2; // Direita
        private static readonly int DOWN = 3; // Baixo

        /// <summary>
        /// Nó de um grafo bidimensional
        /// </summary>
        public class Node
        {
            internal Graph2D graph; // Grafo
            internal int row; // Linha
            internal int col; // Coluna
            internal int value; // Valor (usado no cálculo da distância e na geração do menor caminho entre dois nós do grafo)

            internal Node[] neighbors; // Vizinhos deste nó. São eles 4 valores possíveis: Esquerda, cima, direita e baixo.

            /// <summary>
            /// Cria um novo nó de um grafo bidimensional
            /// </summary>
            /// <param name="graph">Grafo</param>
            /// <param name="row">Linha</param>
            /// <param name="col">Coluna</param>
            internal Node(Graph2D graph, int row, int col)
            {
                this.graph = graph;
                this.row = row;
                this.col = col;

                value = INFINITE; // Define o valor inicialmente como infinito

                // Define os vizinhos inicialmente como nulos
                neighbors = new Node[4];
                for (int i = 0; i < 4; i++)
                    neighbors[i] = null;
            }

            /// <summary>
            /// Grafo ao qual este nó pertence
            /// </summary>
            public Graph2D Graph
            {
                get { return graph; }
            }

            /// <summary>
            /// Linha
            /// </summary>
            public int Row
            {
                get { return row; }
            }

            /// <summary>
            /// Coluna
            /// </summary>
            public int Col
            {
                get { return col; }
            }

            /// <summary>
            /// Coordenadas deste nó (linha e coluna)
            /// </summary>
            public Graph2DCoords Coords
            {
                get { return new Graph2DCoords(row, col); }
            }

            /// <summary>
            /// Valor associado a este nó. Usado para o cálculo de distância e na geração do menor caminho entre dois nós.
            /// </summary>
            public int Value
            {
                get { return value; }
            }

            public override string ToString()
            {
                return (value == INFINITE ? "∞" : value.ToString()) + "[" + row + "," + col + "]";
            }

            /// <summary>
            /// Vizinho à esquerda
            /// </summary>
            public Node Left
            {
                get { return neighbors[LEFT]; }
            }

            internal void SetLeft(Node value)
            {
                neighbors[LEFT] = value;
            }

            /// <summary>
            /// Vizinho acima
            /// </summary>
            public Node Up
            {
                get { return neighbors[UP]; }
            }

            internal void SetUp(Node value)
            {
                neighbors[UP] = value;
            }

            /// <summary>
            /// Vizinho à direita
            /// </summary>
            public Node Right
            {
                get { return neighbors[RIGHT]; }
            }

            internal void SetRight(Node value)
            {
                neighbors[RIGHT] = value;
            }

            /// <summary>
            /// Vizinho abaixo
            /// </summary>
            public Node Down
            {
                get { return neighbors[DOWN]; }
            }

            internal void SetDown(Node value)
            {
                neighbors[DOWN] = value;
            }

            /// <summary>
            /// Atualiza o valor deste nó
            /// </summary>
            /// <param name="value">Valor</param>
            /// <returns></returns>
            internal int UpdateValue(int value)
            {
                this.value = value; // Inicializa com o valor dado
                // Verifica cada vizinho
                for (int i = 0; i < 4; i++)
                {
                    Node neighbor = neighbors[i];
                    if (neighbor == null)
                        continue;

                    // Para todo vizinho que ainda tiver valor infinito
                    if (neighbor.value == INFINITE)
                    {
                        // Atualiza seu valor com base no valor atual incrementado de 1 e obtém um novo valor
                        value = neighbor.UpdateValue(this.value + 1);
                        // Se o novo valor for menor que o valor atual então atualiza o valor atual para este novo valor
                        if (value < this.value)
                            this.value = value + 1;
                    }
                    else if (neighbor.value < this.value) // Senão, se o valor do vizinho for menor que o valor atual...
                        this.value = neighbor.value + 1; // então o valor atual passa a ser o valor do vizinho incrementado de 1
                }

                return this.value;
            }
        }

        private int rowCount; // Número de linhas
        private int colCount; // Número de colunas

        private Node[,] nodes; // Nós
        private int dstRow; // Linha do nó de destino, usado na geração do menor caminho entre dois nós
        private int dstCol; // Coluna do nó de destino, usado na geração do menor caminho entre dois nós
        private bool computed; // Flag usada para indicar que todos os elementos do grafo tiveram seus valores atualizados com as distâncias até o nó de destino informado

        /// <summary>
        /// Cria um novo grafo bidimensional
        /// </summary>
        /// <param name="rowCount">Número de linhas</param>
        /// <param name="colCount">Número de colunas</param>
        /// <param name="fetch">Preenchimento</param>
        public Graph2D(int rowCount, int colCount, bool fetch = true)
        {
            this.rowCount = rowCount;
            this.colCount = colCount;

            nodes = new Node[colCount, rowCount];

            dstRow = -1;
            dstCol = -1;
            computed = false;

            Clear();
            if (fetch) // Se o parâmetro de preenchimento foi especificado como true...
                Fetch(); // preenche todo o grafo
        }

        /// <summary>
        /// Número de linhas
        /// </summary>
        public int RowCount
        {
            get { return rowCount; }
        }

        /// <summary>
        /// Número de colunas
        /// </summary>
        public int ColCount
        {
            get { return colCount; }
        }

        /// <summary>
        /// Nó associado a uma determinada linha e uma determinada coluna
        /// </summary>
        /// <param name="row">Linha</param>
        /// <param name="col">Coluna</param>
        /// <returns>Nó na linha row e coluna col</returns>
        public Node this[int row, int col]
        {
            get { return nodes[col, row]; }
        }

        /// <summary>
        /// Cria um novo nó nas coordenadas especificadas
        /// </summary>
        /// <param name="coords">Coordenadas do novo nó</param>
        /// <returns>Nó criado caso ele não existia, caso contrário retorna o nó existente associado as coordenadas especificadas</returns>
        public Node Insert(Graph2DCoords coords)
        {
            return Insert(coords.Row, coords.Col);
        }

        /// <summary>
        /// Cria um novo nó nas coordenadas especificadas
        /// </summary>
        /// <param name="row">Linha</param>
        /// <param name="col">Coluna</param>
        /// <returns>Nó criado caso ele não existia, caso contrário retorna o nó existente associado as coordenadas especificadas</returns>
        public Node Insert(int row, int col)
        {
            Node node = nodes[col, row];
            if (node != null) // Se já existe um nó nas coordenadas especificadas então retorne este nó
                return node;

            // Senão, cria um novo nó nas coordenadas especificadas
            node = new Node(this, row, col);
            nodes[col, row] = node;

            // Verifica se este novo nó possui uma vizinhança, se possuir então liga-se cada um deles com o novo nó e vice versa
            // Vizinho esquerdo
            if (col > 0)
            {
                Node left = nodes[col - 1, row];
                node.SetLeft(left); // Assim como devemos definir o vizinho deste nó
                if (left != null)
                    left.SetRight(node); // Devemos fazer o mesmo para o vizinho com relação a este nó, trata-se de uma ligação dupla
            }
            // Vizinho acima
            if (row > 0)
            {
                Node up = nodes[col, row - 1];
                node.SetUp(up);
                if (up != null)
                    up.SetDown(node);
            }
            // Vizinho direito
            if (col < colCount - 1)
            {
                Node right = nodes[col + 1, row];
                node.SetRight(right);
                if (right != null)
                    right.SetLeft(node);
            }
            // Vizinho abaixo
            if (row < rowCount - 1)
            {
                Node down = nodes[col, row + 1];
                node.SetDown(down);
                if (down != null)
                    down.SetUp(node);
            }

            computed = false;

            return node;
        }

        /// <summary>
        /// Exclui um nó
        /// </summary>
        /// <param name="coords">Coordenadas do nó</param>
        /// <returns>Nó excluido caso ele exista, null caso contrário</returns>
        public Node Delete(Graph2DCoords coords)
        {
            return Delete(coords.Row, coords.Col);
        }

        /// <summary>
        /// Exclui um nó
        /// </summary>
        /// <param name="row">Linha do nó</param>
        /// <param name="col">Coluna do nó</param>
        /// <returns>Nó excluido caso ele exista, null caso contrário</returns>
        public Node Delete(int row, int col)
        {
            Node node = nodes[col, row];
            if (node == null) // Se o nó não existe, retorne null
                return null;

            nodes[col, row] = null; // Define como null a posição correspondente do nó
            Node[] neighboors = new Node[4];

            // Atualiza todos seus vizinhos, uma vez que o nó está sendo removido então deve-se remover a ligação entre ele e seus vizinhos também.
            // Lembrando que a ligação entre nós vizinhos é dupla, portanto deve-se fazer a atualização dos dois lados.
            // Esquerdo
            if (col > 0)
            {
                node.SetLeft(null);
                Node left = nodes[col - 1, row];
                neighboors[LEFT] = left;
                if (left != null)
                    left.SetRight(null);
            }
            // Acima
            if (row > 0)
            {
                node.SetUp(null);
                Node up = nodes[col, row - 1];
                neighboors[UP] = up;
                if (up != null)
                    up.SetDown(null);
            }
            // Direito
            if (col < colCount - 1)
            {
                node.SetRight(null);
                Node right = nodes[col + 1, row];
                neighboors[RIGHT] = right;
                if (right != null)
                    right.SetLeft(null);
            }
            // Abaixo
            if (row < rowCount - 1)
            {
                node.SetDown(null);
                Node down = nodes[col, row + 1];
                neighboors[DOWN] = down;
                if (down != null)
                    down.SetUp(null);
            }

            computed = false;

            return node;
        }

        /// <summary>
        /// Preenche o grafo com nós em todas as suas posições existentes
        /// </summary>
        public void Fetch()
        {
            for (int row = 0; row < rowCount; row++)
                for (int col = 0; col < colCount; col++)
                    Insert(row, col);

            computed = false;
        }

        /// <summary>
        /// Limpa todos os nós
        /// </summary>
        public void Clear()
        {
            for (int row = 0; row < rowCount; row++)
                for (int col = 0; col < colCount; col++)
                    nodes[col, row] = null;

            computed = false;
        }

        /// <summary>
        /// Preenche os valores de todos os nós do grafo com as distâncias relativas a posição informada.
        /// Para a posição informada a distância é zero e portanto o valor será zero.
        /// Para os nós vizinhos os valores serão 1.
        /// Para os vizinhos dos vizinhos que ainda não tiverem valores definidos a distância será 2.
        /// O preenchimento continua até que todos os nós atíngiveis a partir do nó inicial estejam com seus valores definidos.
        /// Os nós que não forem atingíveis a partir do nó inicial ficarão com seus valores definidos como infinoto.
        /// </summary>
        /// <param name="dst">Coordenadas do nó inicial (nó de destino na geração do menor caminho)</param>
        public void GenerateNodeValues(Graph2DCoords dst)
        {
            GenerateNodeValues(dst.Row, dst.Col);
        }

        /// <summary>
        /// Preenche os valores de todos os nós do grafo com as distâncias relativas a posição informada.
        /// Para a posição informada a distância é zero e portanto o valor será zero.
        /// Para os nós vizinhos os valores serão 1.
        /// Para os vizinhos dos vizinhos que ainda não tiverem valores definidos a distância será 2.
        /// O preenchimento continua até que todos os nós atíngiveis a partir do nó inicial estejam com seus valores definidos.
        /// Os nós que não forem atingíveis a partir do nó inicial ficarão com seus valores definidos como infinoto.
        /// </summary>
        /// <param name="dstRow">Linha do nó inicial (nó de destino na geração do menor caminho)</param>
        /// <param name="dstCol">Coluna do nó inicial (nó de destino na geração do menor caminho)</param>
        public void GenerateNodeValues(int dstRow, int dstCol)
        {
            Node dstNode = nodes[dstCol, dstRow];
            if (dstNode == null)
            {
                this.dstRow = -1;
                this.dstCol = -1;
                computed = false;
                return;
            }

            this.dstRow = dstRow;
            this.dstCol = dstCol;

            // Inicializa todos os nós do grafo com valor infinito
            for (int row = 0; row < rowCount; row++)
                for (int col = 0; col < colCount; col++)
                    if (nodes[col, row] != null)
                        nodes[col, row].value = INFINITE;

            // Começa a atualização dos nós a partir do nó inicial (que posteriormente será usado como nó de destino na geração do menor caminho entre dois nós).
            // O valor do nó inicial começa sempre com zero.
            dstNode.UpdateValue(0);

            computed = true;
        }

        /// <summary>
        /// Obtém o menor caminho entre dois nós
        /// </summary>
        /// <param name="src">Coordenadas do nó de origem</param>
        /// <param name="dst">Coordenadas do nó de destino</param>
        /// <param name="route">Lista contendo os nós do menor caminho, ordenada no sentido da origem para o destino</param>
        /// <returns>true se for encontrado o menor caminho, false caso contrário</returns>
        public bool GetMinimalRoute(Graph2DCoords src, Graph2DCoords dst, List<Graph2DCoords> route)
        {
            return GetMinimalRoute(src.Row, src.Col, dst.Row, dst.Col, route);
        }

        /// <summary>
        /// Obtém o menor caminho entre dois nós
        /// </summary>
        /// <param name="srcRow">Linha do nó de origem</param>
        /// <param name="srcCol">Coluna do nó de origem</param>
        /// <param name="dstRow">Linha do nó de destino</param>
        /// <param name="dstCol">Coluna do nó de destino</param>
        /// <param name="route">Lista contendo os nós do menor caminho, ordenada no sentido da origem para o destino</param>
        /// <returns>true se for encontrado o menor caminho, false caso contrário</returns>
        public bool GetMinimalRoute(int srcRow, int srcCol, int dstRow, int dstCol, List<Graph2DCoords> route)
        {
            if (!computed || this.dstRow != dstRow || this.dstCol != dstCol)
                GenerateNodeValues(dstRow, dstCol);

            return GetMinimalRoute(srcCol, srcCol, route);
        }

        /// <summary>
        /// Obtém o menor caminho entre dois nós. Deve-se chamar o método GenerateNodeValues antes de chamar este método.
        /// </summary>
        /// <param name="coords">Coordenadas do nó de origem</param>
        /// <param name="route">Lista contendo os nós de menor caminho, ordenada no sentido da origem para o destino</param>
        /// <returns>true se for encontrado o menor caminho, false caso contrário ou se o nó de destino não foi informado previamente com a chamada ao método GenerateNodeValues</returns>
        public bool GetMinimalRoute(Graph2DCoords coords, List<Graph2DCoords> route)
        {
            return GetMinimalRoute(coords.Row, coords.Col, route);
        }

        /// <summary>
        /// Obtém o menor caminho entre dois nós. Deve-se chamar o método GenerateNodeValues antes de chamar este método.
        /// </summary>
        /// <param name="srcRow">Linha do nó de origem</param>
        /// <param name="srcCol">Coluna do nó de origem</param>
        /// <param name="route">Lista contendo os nós de menor caminho, ordenada no sentido da origem para o destino</param>
        /// <returns>true se for encontrado o menor caminho, false caso contrário ou se o nó de destino não foi informado previamente com a chamada ao método GenerateNodeValues</returns>
        public bool GetMinimalRoute(int srcRow, int srcCol, List<Graph2DCoords> route)
        {
            if (!computed) // Se ainda não foi computado as distâncias de cada nó do grafo até o nó de destino (o que é feito com a chamada ao método GenerateNodeValues), retorne false
                return false;

            Node dstNode = nodes[dstCol, dstRow];
            if (dstNode == null) // Se o nó de destino não existir, retorne false
                return false;

            Node srcNode = nodes[srcCol, srcRow];
            if (srcNode == null || srcNode.value == INFINITE) // Se o nó de origem não existir ou se seu valor for infinito (o que significa que não existe um caminho que ligue ele até o nó de destino), retorne false 
                return false;

            Node node = srcNode;
            route.Add(node.Coords); // Adiciona as coordenadas do nó de origem como o nó inicial da menor rota
            while (node != dstNode) // Enquanto não chegar no nó de destino
            {
                int value = node.value; // Obtém o valor do nó atual
                Node next = null;
                // Verifica o valor de cada nó vizinho a fim de obter o menor valor entre o valor do nó atual e de seus vizinhos
                for (int i = 0; i < 4; i++)
                {
                    Node neighbor = node.neighbors[i];
                    if (neighbor != null && neighbor.value < value) // Se o valor do nó vizinho for menor que o do nó atual, significa que este vizinho está mais perto do destino
                    {
                        next = neighbor;
                        value = neighbor.value;
                    }
                }

                node = next; // Define o próximo nó como o nó vizinho correspondente ao menor valor (o que estiver mais próximo do nó de destino do caminho)
                if (node == null) // Se este nó não existir, retorne false (Teoricamente isso nunca ocorrerá, mas apenas por segurança fiz essa checagem para evitar um possível loop infinito)
                    return false;

                route.Add(node.Coords); // Adiciona as coordenadas do novo nó a rota
            }

            return true;
        }

        /// <summary>
        /// Obtém o próximo nó a partir do nó de coordenadas coords no qual deve ser seguido para ficar mais próximo do nó de destino (previamente informado pela chamada ao método GenerateNodeValues)
        /// </summary>
        /// <param name="coords">Coordenadas do nó de origem</param>
        /// <returns>Próximo nó do menor caminho entre o nó de coordenadas coords até o nó de destino informado pela chamada ao método GenerateNodeValues, false caso GenerateNodeValues não tenha sido chamado desde a última modificação do grafo</returns>
        public Node GetNextNode(Graph2DCoords coords)
        {
            return GetNextNode(coords.Row, coords.Col);
        }

        /// <summary>
        /// Obtém o próximo nó a partir do nó de coordenadas coords no qual deve ser seguido para ficar mais próximo do nó de destino (previamente informado pela chamada ao método GenerateNodeValues)
        /// </summary>
        /// <param name="srcRow">Linha do nó de origem</param>
        /// <param name="srcCol">Coluna do nó de origem</param>
        /// <returns>Próximo nó do menor caminho entre o nó de coordenadas coords até o nó de destino informado pela chamada ao método GenerateNodeValues, false caso GenerateNodeValues não tenha sido chamado desde a última modificação do grafo</returns>
        public Node GetNextNode(int srcRow, int srcCol)
        {
            if (!computed) // Se ainda não foi computado as distâncias de cada nó do grafo até o nó de destino (o que é feito com a chamada ao método GenerateNodeValues), retorne false
                return null;
            Node dstNode = nodes[dstCol, dstRow];
            if (dstNode == null) // Se o nó de destino não existir, retorne nulo
                return null;

            Node srcNode = nodes[srcCol, srcRow];
            if (srcNode == null || srcNode.value == INFINITE) // Se o nó de origem não existir ou se seu valor for infinito, retorne nulo
                return null;

            if (srcNode.value == 0) // Se o valor do nó de origem for zero, significa que ele é o nó de destino, portanto ele é o próximo nó a ser seguido pelo menor caminho ao nó de destino
                return srcNode;

            // Verifica os valores dos nós vizinhos e obtém o menor deles. O próximo nó deverá ser o nó correspondente a este valor.
            int min = srcNode.value;
            Node result = null;
            for (int i = 0; i < 4; i++)
            {
                Node neighbor = srcNode.neighbors[i];
                if (neighbor != null && neighbor.value < min)
                {
                    result = neighbor;
                    min = neighbor.value;
                }
            }

            return result;
        }

        /// <summary>
        /// Linha do nó de destino (previamente informado a chamada GenerateNodeValues), -1 caso GenerateNodeValues não foi chamada desde a última modificação do grafo
        /// </summary>
        public int DstRow
        {
            get { return computed ? dstRow : -1; }
        }

        /// <summary>
        /// Coluna do nó de destino (previamente informado a chamada GenerateNodeValues), -1 caso GenerateNodeValues não foi chamada desde a última modificação do grafo
        /// </summary>
        public int DstCol
        {
            get { return computed ? dstCol : -1; }
        }

        /// <summary>
        /// Coordenadas do nó de destino (previamente informado a chamada GenerateNodeValues), -1 caso GenerateNodeValues não foi chamada desde a última modificação do grafo
        /// </summary>
        public Graph2DCoords DstCoords
        {
            get { return computed ? new Graph2DCoords(dstRow, dstCol) : null; }
        }
    }
}
