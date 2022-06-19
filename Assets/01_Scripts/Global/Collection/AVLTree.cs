using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ref : https://simpledevcode.wordpress.com/2014/09/16/avl-tree-in-c/
public class AVLTree<T>
{
	protected class Node
	{
		public int key;
		public T value;
		public Node left;
		public Node right;
		public Node(int key, T value)
		{
			this.key = key;
			this.value = value;
		}
	}

	public int Count { get; private set; }
	Node root;

	public AVLTree()
	{
	}

	public virtual void Add(int key, T value)
	{
		Node newItem = new Node(key, value);
		if (root == null)
		{
			root = newItem;
		}
		else
		{
			root = RecursiveInsert(root, newItem);
		}

		Count++;
	}

	private Node RecursiveInsert(Node current, Node n)
	{
		if (current == null)
		{
			current = n;
			return current;
		}
		else if (n.key < current.key)
		{
			current.left = RecursiveInsert(current.left, n);
			current = balance_tree(current);
		}
		else if (n.key > current.key)
		{
			current.right = RecursiveInsert(current.right, n);
			current = balance_tree(current);
		}
		return current;
	}

	private Node balance_tree(Node current)
	{
		int b_factor = balance_factor(current);
		if (b_factor > 1)
		{
			if (balance_factor(current.left) > 0)
			{
				current = RotateLL(current);
			}
			else
			{
				current = RotateLR(current);
			}
		}
		else if (b_factor < -1)
		{
			if (balance_factor(current.right) > 0)
			{
				current = RotateRL(current);
			}
			else
			{
				current = RotateRR(current);
			}
		}
		return current;
	}

	public virtual bool Delete(int target)
	{
		bool isFound = false;

		//and here
		root = Delete(root, target, ref isFound);

		return isFound;
	}

	private Node Delete(Node current, int target, ref bool isFound)
	{
		Node parent;
		if (current == null)
		{ return null; }
		else
		{
			//left subtree
			if (target < current.key)
			{
				current.left = Delete(current.left, target, ref isFound);
				if (balance_factor(current) == -2)//here
				{
					if (balance_factor(current.right) <= 0)
					{
						current = RotateRR(current);
					}
					else
					{
						current = RotateRL(current);
					}
				}
			}
			//right subtree
			else if (target > current.key)
			{
				current.right = Delete(current.right, target, ref isFound);
				if (balance_factor(current) == 2)
				{
					if (balance_factor(current.left) >= 0)
					{
						current = RotateLL(current);
					}
					else
					{
						current = RotateLR(current);
					}
				}
			}
			//if target is found
			else
			{
				if (current.right != null)
				{
					//delete its inorder successor
					parent = current.right;
					while (parent.left != null)
					{
						parent = parent.left;
					}
					current.key = parent.key;
					current.right = Delete(current.right, parent.key, ref isFound);
					if (balance_factor(current) == 2)//rebalancing
					{
						if (balance_factor(current.left) >= 0)
						{
							current = RotateLL(current);
						}
						else { current = RotateLR(current); }
					}

					Count--;
					isFound = true;
				}
				else
				{   //if current.left != null
					return current.left;
				}
			}
		}
		return current;
	}

	public bool Search(int key, out T value)
	{
		bool isEqual = false;

		value = Find(key, root, ref isEqual).value;

		return isEqual;
	}

	private Node Find(int target, Node current, ref bool isEqual)
	{
		if (target < current.key)
		{
			if (target == current.key)
			{
				isEqual = true;
				return current;
			}
			else
				return Find(target, current.left, ref isEqual);
		}
		else
		{
			if (target == current.key)
			{
				isEqual = true;
				return current;
			}
			else
				return Find(target, current.right, ref isEqual);
		}
	}

	public void InOrderLoop(System.Action<T> act)
	{

	}

	protected void InOrderLoop(System.Action<Node> act)
	{

	}

	private void InOrderDisplayTree(Node current, ref bool isContinue)
	{
		if (current != null)
		{
			if (isContinue)
				InOrderDisplayTree(current.left, ref isContinue);

			if (isContinue)
			{

			}

			if (isContinue)
				InOrderDisplayTree(current.right, ref isContinue);
		}
	}

	private int max(int l, int r)
	{
		return l > r ? l : r;
	}

	private int getHeight(Node current)
	{
		int height = 0;
		if (current != null)
		{
			int l = getHeight(current.left);
			int r = getHeight(current.right);
			int m = max(l, r);
			height = m + 1;
		}
		return height;
	}

	private int balance_factor(Node current)
	{
		int l = getHeight(current.left);
		int r = getHeight(current.right);
		int b_factor = l - r;
		return b_factor;
	}

	private Node RotateRR(Node parent)
	{
		Node pivot = parent.right;
		parent.right = pivot.left;
		pivot.left = parent;
		return pivot;
	}

	private Node RotateLL(Node parent)
	{
		Node pivot = parent.left;
		parent.left = pivot.right;
		pivot.right = parent;
		return pivot;
	}

	private Node RotateLR(Node parent)
	{
		Node pivot = parent.left;
		parent.left = RotateRR(pivot);
		return RotateLL(parent);
	}

	private Node RotateRL(Node parent)
	{
		Node pivot = parent.right;
		parent.right = RotateLL(pivot);
		return RotateRR(parent);
	}
}
